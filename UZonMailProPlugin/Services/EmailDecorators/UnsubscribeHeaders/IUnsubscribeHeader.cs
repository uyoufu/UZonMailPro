using MimeKit;

namespace UZonMail.ProPlugin.Services.EmailBodyDecorators.UnsubscribeHeaders
{
    public interface IUnsubscribeHeader
    {
        /// <summary>
        /// 设置头部
        /// </summary>
        /// <param name="mimeMessage"></param>
        void SetHeader(MimeMessage mimeMessage, string unsubscribeUrl);
    }
}
