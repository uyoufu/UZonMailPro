using MimeKit;

namespace UZonMailProPlugin.Services.EmailBodyDecorators.UnsubscribeHeaders
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
