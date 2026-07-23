using DnsClient;
using log4net;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Builder;
using MimeKit;
using System.Globalization;
using System.Net.Sockets;
using System.Text;

namespace UZonMail.ProPlugin.Services.EmailVerify
{
    public class VerifySmtpClient : SmtpClient
    {
        private readonly static ILog _logger = LogManager.GetLogger(typeof(VerifySmtpClient));

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
            catch (SslHandshakeException e)
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
            if (!_existMx)
            {
                return new SmtpResponse(SmtpStatusCode.MailboxUnavailable, "No MX record found");
            }

            if (!IsConnected || !_isConnected)
            {
                return new SmtpResponse(SmtpStatusCode.MailboxUnavailable, "Not connected to SMTP server");
            }

            var toDomain = email.Trim().Split('@').Last();
            var temFromDomains = fromDomains.Where(x => x != toDomain).Take(10).ToList();
            var fromDomain = temFromDomains
                .Skip(new Random().Next(0, temFromDomains.Count))
                .FirstOrDefault();

            // 发送 HELO 命令
            var heloCmd = $"HELO {fromDomain}";
            var helloResponse = await SendCommandAsync(heloCmd);
            if (helloResponse.StatusCode != SmtpStatusCode.Ok)
                return helloResponse;

            // 发送 MAIL FROM 命令
            // 生成随机 a-zA-Z 字符串 6 位
            var randomName = RandomName(6);
            var mailFromCmd = $"MAIL FROM:<{randomName}@{fromDomain}>";
            var mailFromResponse = await SendCommandAsync(mailFromCmd);
            if (mailFromResponse.StatusCode != SmtpStatusCode.Ok)
                return mailFromResponse;

            // 发送 RCPT TO 命令
            var rcptToCmd = $"RCPT TO:<{email}>";
            var rcptToResponse = await SendCommandAsync(rcptToCmd);

            return rcptToResponse;
        }

        private static string RandomName(int length)
        {
            var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
            var random = new Random();
            var name = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                name.Append(chars[random.Next(chars.Length)]);
            }
            return name.ToString();
        }
    }
}
