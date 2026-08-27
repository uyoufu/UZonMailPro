using log4net;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Services.SendCore.Proxies;
using UzonMail.CorePlugin.SignalRHubs;
using UzonMail.CorePlugin.SignalRHubs.Extensions;
using UzonMail.DB.Extensions;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.DB.Utils;
using UzonMail.Utils.Web.Service;

namespace UzonMail.ProPlugin.Services.EmailVerify
{
    /// <summary>
    /// 收件箱验证
    /// </summary>
    public class RecipientContactVerifyService(
        SqlContext db,
        IHubContext<UzonMailHub, IUzonMailClient> hub,
        MxManager mxManager,
        ProxiesManager proxyManager,
        IServiceProvider serviceProvider
    ) : IScopedService
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(RecipientContactVerifyService));

        /// <summary>
        /// 验证收件箱是否有效
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="recipientContacts">必须是被跟踪的ef对象</param>
        /// <returns></returns>
        public async Task Validate(long userId, QueryPaginator<RecipientContact> queryPaginator)
        {
            // 获取所有的发件域名
            var fromDomains = await db.SmtpInfos.Select(x => x.Domain).ToListAsync();
            if (fromDomains.Count == 0)
            {
                _logger.Warn("没有可用的发件域名，请保证 smtpInfos 非空");
                return;
            }

            // 更新用户代理
            await proxyManager.UpdateUserProxies(serviceProvider, userId);

            var client = hub.GetUserClient(userId);
            List<Task> tasks = [];

            // 设置分页大小，防止数据量过大
            queryPaginator.SetPageSize(1000);
            while (true)
            {
                var recipientContacts = await queryPaginator.GetPage().ToListAsync();
                if (recipientContacts == null || recipientContacts.Count == 0)
                {
                    break;
                }

                var task = Task.Run(async () =>
                {
                    // 创建 scope
                    using var scope = serviceProvider.CreateAsyncScope();
                    await ValidateRecipientContacts(
                        fromDomains,
                        recipientContacts,
                        client,
                        userId,
                        scope.ServiceProvider
                    );
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }

        private async Task ValidateRecipientContacts(
            List<string> fromDomains,
            List<RecipientContact> recipientContacts,
            IUzonMailClient hubClient,
            long userId,
            IServiceProvider provider
        )
        {
            Dictionary<string, VerifySmtpClient> smtpClients = [];

            // 获取 smtp 服务器信息
            foreach (var recipientContact in recipientContacts)
            {
                var toDomain = recipientContact.Email.Trim().Split('@')[1];

                var mxRecord = await mxManager.GetRandomMxRecord(toDomain);
                if (string.IsNullOrEmpty(mxRecord))
                {
                    _logger.Info($"获取 MX 记录失败，邮箱: {recipientContact.Email}");
                    // 标记为不可用
                    await db.RecipientContacts.UpdateAsync(
                        x => x.Id == recipientContact.Id,
                        x =>
                            x.SetProperty(
                                    y => y.ValidationStatus,
                                    RecipientValidationStatus.Invalid
                                )
                                .SetProperty(y => y.ValidationFailureReason, "获取 MX 记录失败")
                    );
                    continue;
                }

                var client = new VerifySmtpClient();
                // 匹配代理
                var proxyHander = await proxyManager.GetProxyHander(provider, userId, toDomain);
                if (proxyHander != null)
                    client.ProxyClient = await proxyHander.GetProxyClientAsync(provider, toDomain);

                var connectionOk = await client.ConnectToMx(mxRecord);
                if (!connectionOk)
                {
                    // 说明获取到，但是无法连接，状态未知
                    _logger.Info($"MX 连接失败{mxRecord}");
                    await db.RecipientContacts.UpdateAsync(
                        x => x.Id == recipientContact.Id,
                        x =>
                            x.SetProperty(
                                    y => y.ValidationStatus,
                                    RecipientValidationStatus.Unknown
                                )
                                .SetProperty(y => y.ValidationFailureReason, "MX 连接失败")
                    );
                    continue;
                }

                var response = await client.CheckExist(recipientContact.Email, fromDomains);

                _logger.Debug(
                    $"验证邮箱 {recipientContact.Email} 的结果: {response.StatusCode} - {response.Response}"
                );
                RecipientValidationStatus recipientContactStatus =
                    RecipientValidationStatus.Unverified;
                if (response.StatusCode == SmtpStatusCode.Ok)
                {
                    recipientContactStatus = RecipientValidationStatus.Valid;
                }
                else if (response.StatusCode == SmtpStatusCode.MailboxUnavailable)
                {
                    var responseMsg = response.Response;
                    if (responseMsg.Contains("5.7.1"))
                        recipientContactStatus = RecipientValidationStatus.Unknown;
                    if (responseMsg.Contains("SPF"))
                        recipientContactStatus = RecipientValidationStatus.Unknown;
                    else
                        recipientContactStatus = RecipientValidationStatus.Invalid;
                }
                else
                {
                    recipientContactStatus = RecipientValidationStatus.Unknown;
                }

                recipientContact.ValidationStatus = recipientContactStatus;
                recipientContact.ValidationFailureReason = response.Response;

                // 保存到数据库中
                await db.RecipientContacts.UpdateAsync(
                    x => x.Id == recipientContact.Id,
                    x =>
                        x.SetProperty(y => y.ValidationStatus, recipientContactStatus)
                            .SetProperty(y => y.ValidationFailureReason, response.Response)
                );

                // 推送更新
                await hubClient.RecipientContactStatusChanged(recipientContact);

                await client.DisconnectAsync(true);
            }

            // 保存更新
            await db.SaveChangesAsync();
        }
    }
}
