using log4net;
using Microsoft.EntityFrameworkCore;
using Quartz;
using UZonMail.Core.Services.SendCore;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.EmailSending;
using UZonMail.Utils.Web.Service;
using UZonMailProPlugin.SQL;
using UZonMailProPlugin.SQL.IPWarmUp;

namespace UZonMailProPlugin.Services.IpWarmUp
{
    public class IpWarmUpTaskJob(SqlContextPro dbPro, SqlContext db,
        SendingGroupService sendingService, IpWarmUpTaskService warmUpTaskService) : IJob, IScopedService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(IpWarmUpTaskJob));

        public async Task Execute(IJobExecutionContext context)
        {
            var taskIndex = context.JobDetail.JobDataMap.GetInt("index");
            _logger.Info($" IP 预热计划开始执行,第 {taskIndex + 1} 次任务");

            // 获取组 id
            var warmUpPlanId = context.JobDetail.JobDataMap.GetInt("id");
            if (warmUpPlanId == 0)
            {
                _logger.Warn("IP 预热计划 id 为空");
                return;
            }

            var plan = await dbPro.IpWarmUpUpPlans.FirstOrDefaultAsync(x => x.Id == warmUpPlanId);
            if (plan == null)
            {
                _logger.Warn($"未能匹配到 IP 预热计划: {warmUpPlanId}");
                return;
            }

            // 根据当前发件的状态，构建发送组
            if (plan.CurrentInboxIndex >= plan.InboxIds.Count)
            {
                _logger.Info($"IP 预热计划 {plan.Id} 已经完成所有任务");
                return;
            }

            // 获取当前任务，若仍在进行中，则下一次再执行
            var lastPlanTask = await dbPro.IpWarmUpUpTasks
                .Where(x => x.IPWarmUpPlanId == plan.Id)
                .OrderByDescending(x => x.Id)
                .Take(1)
                .Include(x => x.SendingGroup)
                .FirstOrDefaultAsync();

            var isSending = false;
            // 获取最后一次任务
            if (lastPlanTask != null)
            {
                isSending = lastPlanTask.SendingGroup.Status != SendingGroupStatus.Cancel
                    && lastPlanTask.SendingGroup.Status != SendingGroupStatus.Finish;
            }

            // 获取密钥
            string[] smtpPasswordSecretKeys = context.JobDetail.JobDataMap.GetString("smtpPasswordSecretKeys")!.Split(',');
            // 如果在发送中，则跳过
            if (!isSending)
            {
                await CreateWarmUpTaskAndSending(plan, smtpPasswordSecretKeys);
            }

            if(plan.CurrentInboxIndex >= plan.InboxIds.Count)
            {
                _logger.Info($"IP 预热计划 {plan.Id} 已经完成所有任务");
                return;
            }
            // 定制下一次任务
            await warmUpTaskService.CreatePlanSchedule(plan.Id, smtpPasswordSecretKeys, new DateTimeOffset(DateTime.UtcNow.AddDays(1)), taskIndex + 1);
        }

        private async Task CreateWarmUpTaskAndSending(IpWarmUpUpPlan plan, string[] smtpPasswordSecretKeys)
        {
            var sendingGroup = new SendingGroup
            {
                SmtpPasswordSecretKeys = [.. smtpPasswordSecretKeys],
            };
            await sendingService.SendNow(sendingGroup);
        }
    }
}
