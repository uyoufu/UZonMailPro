using Microsoft.EntityFrameworkCore;
using Quartz;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.SQL.Core.EmailSending;
using UzonMail.ProPlugin.Controllers.IPWarmUp.DTOs;
using UzonMail.ProPlugin.SQL;
using UzonMail.ProPlugin.SQL.IPWarmUp;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.Service;

namespace UzonMail.ProPlugin.Services.IpWarmUp
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

            var senderAccountSet = new HashSet<long>(plandData.SenderAccounts!.Select(x => x.Id));
            var recipientContactSet = new HashSet<long>(plandData.Recipients!.Select(x => x.Id));

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
            if (plandData.SenderAccountGroups != null && plandData.SenderAccountGroups.Count > 0)
            {
                var senderAccounts = await db
                    .SenderAccounts.AsNoTracking()
                    .Where(x => plandData.SenderAccountGroups.Select(x => x.Id).Contains(x.Id))
                    .Select(x => new { x.Id })
                    .ToListAsync();
                senderAccounts.ForEach(x => senderAccountSet.Add(x.Id));
            }

            // 获取组中的收件箱
            if (
                plandData.RecipientContactGroups != null
                && plandData.RecipientContactGroups.Count > 0
            )
            {
                var recipientContacts = await db
                    .RecipientContacts.AsNoTracking()
                    .Where(x => plandData.RecipientContactGroups.Select(x => x.Id).Contains(x.Id))
                    .Select(x => new { x.Id })
                    .ToListAsync();
                recipientContacts.ForEach(x => recipientContactSet.Add(x.Id));
            }

            // 添加数据中的发件箱和收件箱
            if (plandData.Data != null)
            {
                var excelData = new ExcelDataInfo(plandData.Data);
                // 数据中的发件箱为特定发件箱，不添加到全局

                // 获取数据中的收件箱
                // 若不存在，则创建
                foreach (var recipientContact in excelData.RecipientEmails)
                {
                    var existRecipientContact = await db
                        .RecipientContacts.AsNoTracking()
                        .Where(x => x.UserId == userId && x.Email == recipientContact)
                        .Select(x => new { x.Id })
                        .FirstOrDefaultAsync();
                    if (existRecipientContact != null)
                    {
                        recipientContactSet.Add(existRecipientContact.Id);
                        continue;
                    }

                    // 添加新的收件箱
                    var newRecipientContact = new RecipientContact()
                    {
                        UserId = userId,
                        Email = recipientContact,
                        Name = "",
                        CreateDate = DateTime.UtcNow
                    };
                    await db.RecipientContacts.AddAsync(newRecipientContact);
                    await db.SaveChangesAsync();
                    recipientContactSet.Add(newRecipientContact.Id);
                }
            }

            plan.SenderAccountIds = [.. senderAccountSet];
            plan.RecipientContactIds = [.. recipientContactSet];

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
