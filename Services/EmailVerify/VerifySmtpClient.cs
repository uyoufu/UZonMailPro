using System.Net.Sockets;
using System.Text;
using log4net;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace UzonMail.ProPlugin.Services.EmailVerify
{
    public class VerifySmtpClient : SmtpClient
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(VerifySmtpClient));

        private bool _existMx = true;
        private bool _isConnected = false;

        private readonly Queue<Tuple<int, SecureSocketOptions>> _retryItems = [];

        public VerifySmtpClient()
        {
            _retryItems.Enqueue(new(25, SecureSocketOptions.Auto));
            _retryItems.Enqueue(new(25, SecureSocketOptions.None));
            _retryItems.Enqueue(new(465, SecureSocketOptions.Auto));
            _retryItems.Enqueue(new(465, SecureSocketOptions.None));
            _retryItems.Enqueue(new(587, SecureSocketOptions.StartTls));
            _retryItems.Enqueue(new(587, SecureSocketOptions.None));
        }

        /// <summary>
        /// 使用 mx 进行连接
        /// </summary>
        /// <param name="mxRecord"></param>
        /// <returns></returns>
        public async Task<bool> ConnectToMx(string mxRecord)
        {
            try
            {
                if (_retryItems.Count == 0)
                {
                    return false;
                }

                var option = _retryItems.Dequeue();

                await ConnectAsync(mxRecord, option.Item1, option.Item2);
                _isConnected = true;
                return true;
            }
            catch (SslHandshakeException)
            {
                // 说明 ssl 连接失败
                return await ConnectToMx(mxRecord);
            }
            catch (SmtpCommandException e)
            {
                _logger.Error(e.GetType());
                _logger.Warn(e);
            }
            catch (SocketException e)
            {
                _logger.Error(e.GetType());
                _logger.Warn(e);
            }
            catch (Exception e)
            {
                _logger.Error(e.GetType());
                _logger.Warn(e);
            }

            return false;
        }

        /// <summary>
        /// 验证是否存在
        /// </summary>
        /// <returns></returns>
        public async Task<SmtpResponse> CheckExist(string email, List<string> fromDomains)
        {
            var toDomain = email.Trim().Split('@').Last();
            var temFromDomains = fromDomains.Where(x => x != toDomain).Take(10).ToList();
            var fromDomain =
                temFromDomains.Count > 0
                    ? temFromDomains[Random.Shared.Next(temFromDomains.Count)]
                    : fromDomains.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(fromDomain))
                return new SmtpResponse(SmtpStatusCode.MailboxUnavailable, "未提供可用的发件域名");

            return (await ProbeRecipientAsync(email, fromDomain)).Response;
        }

        /// <summary>
        /// 探测收件人地址并保留 SMTP 拒绝所在的命令阶段
        /// </summary>
        public async Task<SmtpProbeResult> ProbeRecipientAsync(string email, string fromDomain)
        {
            if (!_existMx)
                return CreateUnavailableResult("No MX record found");

            if (!IsConnected || !_isConnected)
                return CreateUnavailableResult("Not connected to SMTP server");

            var heloCmd = $"HELO {fromDomain}";
            var helloResponse = await SendCommandAsync(heloCmd);
            if (helloResponse.StatusCode != SmtpStatusCode.Ok)
                return new SmtpProbeResult(SmtpProbeStage.Helo, helloResponse);

            var randomName = RandomName(6);
            var mailFromCmd = $"MAIL FROM:<{randomName}@{fromDomain}>";
            var mailFromResponse = await SendCommandAsync(mailFromCmd);
            if (mailFromResponse.StatusCode != SmtpStatusCode.Ok)
                return new SmtpProbeResult(SmtpProbeStage.MailFrom, mailFromResponse);

            var rcptToCmd = $"RCPT TO:<{email}>";
            var rcptToResponse = await SendCommandAsync(rcptToCmd);

            return new SmtpProbeResult(SmtpProbeStage.Recipient, rcptToResponse);
        }

        private static SmtpProbeResult CreateUnavailableResult(string reason) =>
            new(SmtpProbeStage.Helo, new SmtpResponse(SmtpStatusCode.MailboxUnavailable, reason));

        private static string RandomName(int length)
        {
            var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
            var name = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                name.Append(chars[Random.Shared.Next(chars.Length)]);
            }
            return name.ToString();
        }
    }
}
