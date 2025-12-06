using Microsoft.EntityFrameworkCore;
using Quartz;
using UZonMail.CorePlugin.Services.Settings;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.DB.SQL.Core.EmailSending;
using UZonMail.Utils.Web.Exceptions;
using UZonMail.Utils.Web.Service;
using UZonMail.ProPlugin.Controllers.IPWarmUp.DTOs;
using UZonMail.ProPlugin.SQL;
using UZonMail.ProPlugin.SQL.IPWarmUp;

namespace UZonMail.ProPlugin.Services.IpWarmUp
{
    /// <summary>
    /// IP 预热任务
    /// </summary>
    /// <param name="dbPro"></param>
    public class IpWarmUpTaskService(
        SqlContext db,
        SqlContextPro dbPro,
        TokenService tokenService,
        ISchedulerFactory schedulerFactory
    ) : IScopedService
    {
        /// <summary>
        /// 创建预热计划
        /// </summary>
        /// <param name="plandData"></param>
        /// <returns></returns>
        public async Task<IpWarmUpUpPlan> CreatePlan(WarmUpPlanData plandData)
        {
            var userId = tokenService.GetUserSqlId();

            var outboxSet = new HashSet<long>(plandData.Outboxes!.Select(x => x.Id));
            var inboxSet = new HashSet<long>(plandData.Inboxes!.Select(x => x.Id));

            // 将 sendingGroup 转换成 IpWarmUpPlan 保存
            var plan = new IpWarmUpUpPlan()
            {
                UserId = userId,
                Body = plandData.Body,
                Subjects = plandData.SplitSubjects(),
                StartDate = plandData.SendStartDate.ToUniversalTime(),
                EndDate = plandData.SendEndDate.ToUniversalTime(),
                Name = string.IsNullOrEmpty(plandData.Name) ? plandData.Subjects : plandData.Name,
                Status = IpWarmUpUpStatus.Created,
                CreateDate = DateTime.UtcNow,
                Data = plandData.Data,
                TemplateIds = [.. plandData.Templates!.Select(x => x.Id)],
                CcIds = [.. plandData.CcBoxes!.Select(x => x.Id)],
                BccIds = [.. plandData.BccBoxes!.Select(x => x.Id)],
                AttachmentIds = [.. plandData.Attachments!.Select(x => x.Id)],
            };

            // 获取组中的发件箱
            if (plandData.OutboxGroups != null && plandData.OutboxGroups.Count > 0)
            {
                var outboxes = await db
                    .Outboxes.AsNoTracking()
                    .Where(x => plandData.OutboxGroups.Select(x => x.Id).Contains(x.Id))
                    .Select(x => new { x.Id })
                    .ToListAsync();
                outboxes.ForEach(x => outboxSet.Add(x.Id));
            }

            // 获取组中的收件箱
            if (plandData.InboxGroups != null && plandData.InboxGroups.Count > 0)
            {
                var inboxes = await db
                    .Inboxes.AsNoTracking()
                    .Where(x => plandData.InboxGroups.Select(x => x.Id).Contains(x.Id))
                    .Select(x => new { x.Id })
                    .ToListAsync();
                inboxes.ForEach(x => inboxSet.Add(x.Id));
            }

            // 添加数据中的发件箱和收件箱
            if (plandData.Data != null)
            {
                var excelData = new ExcelDataInfo(plandData.Data);
                // 数据中的发件箱为特定发件箱，不添加到全局

                // 获取数据中的收件箱
                // 若不存在，则创建
                foreach (var inbox in excelData.InboxSet)
                {
                    var existInbox = await db
                        .Inboxes.AsNoTracking()
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

            // 添加发送图表
            plan.SendCountChartPoints = plandData.SendCountChartPoints;

            // 保存预热计划
            await dbPro.IpWarmUpUpPlans.AddAsync(plan);
            await dbPro.SaveChangesAsync();

            // 创建循环任务，每天都执行发送任务
            await CreatePlanSchedule(plan.Id, plan.StartDate, plan.EndDate);

            return plan;
        }

        /// <summary>
        /// 创建预热计划的定时任务 Key
        /// </summary>
        /// <param name="planId"></param>
        /// <returns></returns>
        public JobKey CreateWarmUpPlanScheduleJobKey(long planId)
        {
            return new JobKey($"IpWarmUpPlan_{planId}", "IpWarmUpPlan");
        }

        /// <summary>
        /// 创建预热计划的定时任务
        /// </summary>
        /// <param name="planId"></param>
        /// <param name="smtpPasswordSecretKeys"></param>
        /// <param name="startDateUtc"></param>
        /// <param name="endDateUtc"></param>
        /// <returns></returns>
        public async Task CreatePlanSchedule(
            long planId,
            DateTime startDateUtc,
            DateTime endDateUtc
        )
        {
            // 添加定时任务, 让定时任务去执行具体的发送任务
            var scheduler = await schedulerFactory.GetScheduler();
            var jobKey = CreateWarmUpPlanScheduleJobKey(planId);

            var job = JobBuilder
                .Create<IpWarmUpTaskJob>()
                .WithIdentity(jobKey)
                .SetJobData(new JobDataMap { { "id", planId }, })
                .Build();

            if (startDateUtc < DateTime.UtcNow.AddSeconds(10))
                startDateUtc = DateTime.UtcNow.AddSeconds(10);

            // 创建触发器，每天执行一次
            var trigger = TriggerBuilder
                .Create()
                .ForJob(jobKey)
                .StartAt(new DateTimeOffset(startDateUtc))
                .EndAt(new DateTimeOffset(endDateUtc))
                .WithCalendarIntervalSchedule(x =>
                    x.WithIntervalInDays(1)
                        // 保留每日触发的小时（在夏时制切换时更稳定）
                        .PreserveHourOfDayAcrossDaylightSavings(true)
                )
                .Build();
            await scheduler.ScheduleJob(job, trigger);
        }

        /// <summary>
        /// 删除预热计划的定时任务
        /// </summary>
        /// <param name="planId"></param>
        /// <returns></returns>
        public async Task<bool> DeletePlanSchedule(long planId)
        {
            // 删除定时任务
            var scheduler = schedulerFactory.GetScheduler().Result;
            var jobKey = CreateWarmUpPlanScheduleJobKey(planId);
            return await scheduler.DeleteJob(jobKey);
        }

        /// <summary>
        /// 暂停预热计划的定时任务
        /// </summary>
        /// <param name="planId"></param>
        /// <returns></returns>
        public async Task PausePlanSchedule(long planId)
        {
            var scheduler = schedulerFactory.GetScheduler().Result;
            var jobKey = CreateWarmUpPlanScheduleJobKey(planId);
            await scheduler.PauseJob(jobKey);
        }

        /// <summary>
        /// 重新启动预热计划的定时任务
        /// </summary>
        /// <param name="planId"></param>
        /// <returns></returns>
        public async Task ResumePlanSchedule(long planId)
        {
            var scheduler = schedulerFactory.GetScheduler().Result;
            var jobKey = CreateWarmUpPlanScheduleJobKey(planId);
            await scheduler.ResumeJob(jobKey);
        }
    }
}
