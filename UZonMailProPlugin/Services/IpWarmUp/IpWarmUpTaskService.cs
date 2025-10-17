using Microsoft.EntityFrameworkCore;
using Quartz;
using UZonMail.Core.Services.Settings;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.DB.SQL.Core.EmailSending;
using UZonMail.Utils.Web.Exceptions;
using UZonMail.Utils.Web.Service;
using UZonMailProPlugin.SQL;
using UZonMailProPlugin.SQL.IPWarmUp;

namespace UZonMailProPlugin.Services.IpWarmUp
{
    /// <summary>
    /// IP 预热任务
    /// </summary>
    /// <param name="dbPro"></param>
    public class IpWarmUpTaskService(SqlContext db, SqlContextPro dbPro, TokenService tokenService, ISchedulerFactory schedulerFactory) : IScopedService
    {
        /// <summary>
        /// 创建预热计划
        /// </summary>
        /// <param name="sendingGroup"></param>
        /// <returns></returns>
        public async Task<IpWarmUpUpPlan> CreatePlan(SendingGroup sendingGroup)
        {
            if ((sendingGroup.SmtpPasswordSecretKeys == null || sendingGroup.SmtpPasswordSecretKeys.Count != 2))
            {
                throw new KnownException("请提供两个 SMTP 密钥，用于不同阶段的发送");
            }

            var userId = tokenService.GetUserSqlId();

            var outboxSet = new HashSet<long>(sendingGroup.Outboxes!.Select(x => x.Id));
            var inboxSet = new HashSet<long>(sendingGroup.Inboxes!.Select(x => x.Id));

            // 将 sendingGroup 转换成 IpWarmUpPlan 保存
            var plan = new IpWarmUpUpPlan()
            {
                UserId = userId,
                Subjects = sendingGroup.Subjects,
                StartDate = DateTime.UtcNow,
                Name = sendingGroup.Subjects,
                Status = IpWarmUpUpStatus.Created,
                CreateDate = DateTime.UtcNow,
                Data = sendingGroup.Data,
                TemplateIds = [.. sendingGroup.Templates!.Select(x => x.Id)],
                CcIds = [.. sendingGroup.CcBoxes!.Select(x => x.Id)],
                BccIds = [.. sendingGroup.BccBoxes!.Select(x => x.Id)],
            };

            // 获取组中的发件箱
            if (sendingGroup.OutboxGroups != null && sendingGroup.OutboxGroups.Count > 0)
            {
                var outboxes = await db.Outboxes.AsNoTracking()
                    .Where(x => sendingGroup.OutboxGroups.Select(x => x.Id).Contains(x.Id))
                    .Select(x => new { x.Id })
                    .ToListAsync();
                outboxes.ForEach(x => outboxSet.Add(x.Id));
            }

            // 获取组中的收件箱
            if (sendingGroup.InboxGroups != null && sendingGroup.InboxGroups.Count > 0)
            {
                var inboxes = await db.Inboxes.AsNoTracking()
                    .Where(x => sendingGroup.InboxGroups.Select(x => x.Id).Contains(x.Id))
                    .Select(x => new { x.Id })
                    .ToListAsync();
                inboxes.ForEach(x => inboxSet.Add(x.Id));
            }

            // 添加数据中的发件箱和收件箱
            if (sendingGroup.Data != null)
            {
                var excelData = new ExcelDataInfo(sendingGroup.Data);
                // 数据中的发件箱为特定发件箱，不添加到全局

                // 获取数据中的收件箱
                // 若不存在，则创建
                foreach (var inbox in excelData.InboxSet)
                {
                    var existInbox = await db.Inboxes.AsNoTracking()
                        .Where(x => x.UserId == userId && x.Email == inbox)
                        .Select(x => new { x.Id })
                        .FirstOrDefaultAsync();
                    if (existInbox != null)
                    {
                        inboxSet.Add(existInbox.Id);
                        continue;
                    }

                    // 添加新的收件箱
                    var newInbox = new Inbox()
                    {
                        UserId = userId,
                        Email = inbox,
                        Name = "",
                        CreateDate = DateTime.UtcNow
                    };
                    await db.Inboxes.AddAsync(newInbox);
                    await db.SaveChangesAsync();
                    inboxSet.Add(newInbox.Id);
                }
            }

            plan.OutboxIds = [.. outboxSet];
            plan.InboxIds = [.. inboxSet];

            // 保存预热计划
            await dbPro.IpWarmUpUpPlans.AddAsync(plan);
            await dbPro.SaveChangesAsync();

            await CreatePlanSchedule(plan.Id, [.. sendingGroup.SmtpPasswordSecretKeys], new DateTimeOffset(DateTime.UtcNow.AddSeconds(10)), 0);

            return plan;
        }

        public async Task CreatePlanSchedule(long planId, string[] smtpPasswordSecretKeys, DateTimeOffset dateTimeOffset, int index)
        {
            // 添加定时任务, 让定时任务去执行具体的发送任务
            var scheduler = await schedulerFactory.GetScheduler();
            var jobKey = new JobKey($"IpWarmUpPlan_{planId}", "IpWarmUpPlan");

            var job = JobBuilder.Create<IpWarmUpTaskJob>()
                        .WithIdentity(jobKey)
                        .SetJobData(new JobDataMap
                        {
                            { "id",planId },
                            { "index",index},
                            { "smtpPasswordSecretKeys", string.Join(',',smtpPasswordSecretKeys) }
                        })
                        .Build();

            // 先指定为 Unspecified，再转为本地时间
            var trigger = TriggerBuilder.Create()
                .ForJob(jobKey)
                .StartAt(dateTimeOffset)
                .Build();
            await scheduler.ScheduleJob(job, trigger);
        }
    }
}
