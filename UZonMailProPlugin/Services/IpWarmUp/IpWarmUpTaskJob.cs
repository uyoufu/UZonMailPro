using log4net;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Quartz;
using UZonMail.Core.Services.SendCore;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.EmailSending;
using UZonMail.Utils.Json;
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

            // 获取当前任务，若仍在进行中，则下一次再执行
            var lastPlanTask = await dbPro.IpWarmUpUpTasks
                .Where(x => x.IPWarmUpPlanId == plan.Id)
                .OrderByDescending(x => x.Id)
                .Take(1)
                .FirstOrDefaultAsync();

            var isSending = false;
            // 获取最后一次任务
            if (lastPlanTask != null)
            {
                // 判断是否在发送中
                var sendingGroup = await db.SendingGroups.AsNoTracking().Where(x => x.Id == lastPlanTask.SendingGroupId)
                    .Select(x => new SendingGroup
                    {
                        Status = x.Status
                    })
                    .FirstAsync();

                isSending = sendingGroup.Status != SendingGroupStatus.Cancel
                    && sendingGroup.Status != SendingGroupStatus.Finish;
            }

            // 获取密钥
            string[] smtpPasswordSecretKeys = context.JobDetail.JobDataMap.GetString("smtpPasswordSecretKeys")!.Split(',');
            // 如果在发送中，则跳过
            if (!isSending)
            {
                await CreateWarmUpTaskAndSending(plan, smtpPasswordSecretKeys);
            }

            // 判断是否需要定制下一次任务
            if (plan.EndDate < DateTime.UtcNow.AddDays(1))
            {
                _logger.Info($"IP 预热计划 {plan.Id} 完成：已经至截止日期");
                return;
            }

            // 定制下一次任务
            await warmUpTaskService.CreatePlanSchedule(plan.Id, smtpPasswordSecretKeys, new DateTimeOffset(DateTime.UtcNow.AddDays(1)), taskIndex + 1);
        }

        private async Task CreateWarmUpTaskAndSending(IpWarmUpUpPlan plan, string[] smtpPasswordSecretKeys)
        {
            _logger.Info($"IP 预热计划 {plan.Id} 创建发送任务");

            // 生成发送组数据
            var sendingGroup = new SendingGroup
            {
                SmtpPasswordSecretKeys = [.. smtpPasswordSecretKeys],
                Subjects = string.Join("\n", plan.Subjects),

                // 获取模板
                Templates = plan.TemplateIds.Count > 0 ? await db.EmailTemplates
                    .Where(x => plan.TemplateIds.Contains(x.Id))
                    .ToListAsync() : [],

                // 获取 outbox
                Outboxes = plan.OutboxIds.Count > 0 ? await db.Outboxes
                    .Where(x => plan.OutboxIds.Contains(x.Id))
                    .ToListAsync() : [],

                // 获取 cc
                CcBoxes = plan.CcIds.Count > 0 ? await db.Inboxes
                    .Where(x => plan.CcIds.Contains(x.Id))
                    .Select(x => new EmailAddress()
                    {
                        Id = x.Id,
                        ObjectId = x.ObjectId,
                        Email = x.Email,
                        Name = x.Name
                    })
                    .ToListAsync() : [],

                // 获取 bcc
                BccBoxes = plan.BccIds.Count > 0 ? await db.Inboxes
                    .Where(x => plan.BccIds.Contains(x.Id))
                    .Select(x => new EmailAddress()
                    {
                        Id = x.Id,
                        ObjectId = x.ObjectId,
                        Email = x.Email,
                        Name = x.Name
                    })
                    .ToListAsync() : [],

                // 附件
                Attachments = plan.AttachmentIds.Count > 0 ? await db.FileUsages
                    .Where(x => plan.AttachmentIds.Contains(x.Id))
                    .ToListAsync() : [],
            };

            // 获取 inbox
            var countCalculator = new DailySendCountCalculator(plan.StartDate, plan.EndDate, plan.InboxIds.Count, plan.SendCountChartPoints);
            var todaySendCount = countCalculator.GetCountForToday();
            _logger.Info($"IP 预热计划 {plan.Id} 今日发送量为 {todaySendCount}");
            if (todaySendCount == 0)
            {
                return;
            }

            // 随机生成收件箱索引
            var randInboxIds = FisherYatesSample(plan.InboxIds, todaySendCount);
            sendingGroup.Inboxes = await db.Inboxes
                .Where(x => randInboxIds.Contains(x.Id))
                .Select(x => new EmailAddress()
                {
                    Id = x.Id,
                    ObjectId = x.ObjectId,
                    Email = x.Email,
                    Name = x.Name
                })
                .ToListAsync();

            // 获取数据
            if(plan.Data != null)
            {
                var newArr = new JArray();
                foreach (var row in plan.Data)
                {
                    var inbox = row.SelectTokenOrDefault("inbox", string.Empty);
                    if (string.IsNullOrEmpty(inbox)) continue;

                    var existInbox = sendingGroup.Inboxes.FirstOrDefault(x => x.Email == inbox);
                    if (existInbox != null)
                    {
                        newArr.Add(row.DeepClone());
                    }
                }
                sendingGroup.Data = newArr;
            }

            // 保存到数据库
            db.SendingGroups.Add(sendingGroup);
            await db.SaveChangesAsync();

            await sendingService.SendNow(sendingGroup);
        }

        public static List<T> FisherYatesSample<T>(IEnumerable<T> source, int k, Random? rng = null)
        {
            ArgumentNullException.ThrowIfNull(source);
            rng ??= Random.Shared;
            var arr = source.ToArray();
            int n = arr.Length;
            if (k >= n) return [.. arr];

            // 完整洗牌（也可以只完成 n-1..n-k 的交换）
            for (int i = n - 1; i > 0; i--)
            {
                int j = rng.Next(0, i + 1);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }

            return [.. source.Take(k)];
        }
    }
}
