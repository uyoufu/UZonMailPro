using log4net;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UZonMail.Core.SignalRHubs;
using UZonMail.Core.SignalRHubs.Extensions;
using UZonMail.DB.Extensions;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.DB.Utils;
using UZonMail.Utils.Web.Service;

namespace UZonMailProPlugin.Services.EmailVerify
{
    /// <summary>
    /// 收件箱验证
    /// </summary>
    public class InboxVerifyService(SqlContext db, IHubContext<UzonMailHub, IUzonMailClient> hub, MxManager mxManager) : ITransientService
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(InboxVerifyService));

        /// <summary>
        /// 验证收件箱是否有效
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="inboxes">必须是被跟踪的ef对象</param>
        /// <returns></returns>
        public async Task Validate(long userId, QueryPaginator<Inbox> queryPaginator)
        {
            // 获取所有的发件域名
            var fromDomains = await db.SmtpInfos.Select(x => x.Domain).ToListAsync();
            if (fromDomains.Count == 0)
            {
                _logger.Warn("没有可用的发件域名，请保证 smtpInfos 非空");
                return;
            }

            var client = hub.GetUserClient(userId);
            List<Task> tasks = [];

            // 设置分页大小，防止数据量过大
            queryPaginator.SetPageSize(1000);
            while (true)
            {
                var inboxes = await queryPaginator.GetPage().ToListAsync();
                if (inboxes == null || inboxes.Count == 0)
                {
                    break;
                }

                var task = Task.Run(async () =>
                {
                    // 创建 scope
                    await ValidateInboxes(fromDomains, inboxes, client);
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        private async Task ValidateInboxes(List<string> fromDomains, List<Inbox> inboxes, IUzonMailClient hubClient)
        {
            Dictionary<string, VerifySmtpClient> smtpClients = [];

            // 获取 smtp 服务器信息
            foreach (var inbox in inboxes)
            {
                var toDomain = inbox.Email.Trim().Split('@')[1];

                var mxRecord = await mxManager.GetRandomMxRecord(toDomain);
                if (string.IsNullOrEmpty(mxRecord))
                {
                    _logger.Info($"获取 MX 记录失败，邮箱: {inbox.Email}");
                    // 标记为不可用
                    await db.Inboxes.UpdateAsync(x => x.Id == inbox.Id, x => x.SetProperty(y => y.Status, InboxStatus.Invalid)
                        .SetProperty(y => y.ValidFailReason, "获取 MX 记录失败"));
                    continue;
                }

                var client = new VerifySmtpClient();
                var connectionOk = await client.ConnectToMx(mxRecord);
                if (!connectionOk)
                {
                    // 说明获取到，但是无法连接，状态未知
                    _logger.Info($"MX 连接失败{mxRecord}");
                    await db.Inboxes.UpdateAsync(x => x.Id == inbox.Id, x => x.SetProperty(y => y.Status, InboxStatus.Unkown)
                        .SetProperty(y => y.ValidFailReason, "MX 连接失败"));
                    continue;
                }

                var response = await client.CheckExist(inbox.Email, fromDomains);

                _logger.Debug($"验证邮箱 {inbox.Email} 的结果: {response.StatusCode} - {response.Response}");

                // 保存到数据库中
                await db.Inboxes.UpdateAsync(x => x.Id == inbox.Id, x => x.SetProperty(y => y.Status, response.StatusCode == SmtpStatusCode.Ok ? InboxStatus.Valid : InboxStatus.Invalid)
                    .SetProperty(y => y.ValidFailReason, response.Response));

                // 推送更新
                await hubClient.InboxStatusChanged(inbox);

                await client.DisconnectAsync(true);
            }

            // 保存更新
            await db.SaveChangesAsync();
        }
    }
}
