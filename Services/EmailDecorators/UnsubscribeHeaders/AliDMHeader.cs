using MimeKit;
using Newtonsoft.Json.Linq;

namespace UZonMail.ProPlugin.Services.EmailBodyDecorators.UnsubscribeHeaders
{
    public class AliDMHeader : RFC8058Header
    {
        public override void SetHeader(MimeMessage mimeMessage, string unsubscribeUrl)
        {
            base.SetHeader(mimeMessage, unsubscribeUrl);

            // 添加阿里云DM退订头
            // 参考：https://help.aliyun.com/zh/direct-mail/smtp-controls-the-specified-function-through-the-configuration-item-header?spm=a2c4g.11186623.0.0.3c0815a4uZ6pqG
            var headerValue = AliHeaderModel.GetHeaderValue();
            mimeMessage.Headers.Add("X-AliDM-Settings", headerValue);
        }
    }
}
