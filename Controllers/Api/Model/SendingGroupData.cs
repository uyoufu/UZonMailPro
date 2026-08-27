using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Base;
using UzonMail.DB.SQL.Core.EmailSending;

namespace UzonMail.ProPlugin.Controllers.Api.Model
{
    public class SendingGroupData
    {
        /// <summary>
        /// 主题
        /// 多个主题使用分号或者换行分隔
        /// </summary>
        public List<string> Subjects { get; set; } = [];

        /// <summary>
        /// 需要使用的模板 Ids
        /// </summary>
        public List<string>? TemplateIds { get; set; } = [];

        /// <summary>
        /// 正文内容
        /// </summary>
        public string? Body { get; set; }

        /// <summary>
        /// 发件箱 id
        /// </summary>
        public List<string> SenderAccountIds { get; set; } = [];

        /// <summary>
        /// 发件箱组的 Id
        /// </summary>
        public List<string> SenderAccountGroupIds { get; set; } = [];

        /// <summary>
        /// 收件邮箱
        /// 为邮箱地址
        /// </summary>
        public List<string> RecipientEmails { get; set; } = [];

        /// <summary>
        /// 收件箱组
        /// </summary>
        public List<string>? RecipientContactGroupIds { get; set; } = [];

        /// <summary>
        /// 抄送箱
        /// </summary>
        public List<string>? CcBoxes { get; set; } = [];

        /// <summary>
        /// 密送
        /// </summary>
        public List<string>? BccBoxes { get; set; } = [];

        /// <summary>
        /// 上传附件需要的 Ids
        /// </summary>
        public List<string>? AttachmentIds { get; set; } = [];

        /// <summary>
        /// 用户通过 excel 上传的数据
        /// </summary>
        public JArray? Data { get; set; }

        #region 定时发件相关
        /// <summary>
        /// 发件类型
        /// </summary>
        public SendingGroupType SendingType { get; set; }

        /// <summary>
        /// 定时发件时间
        /// </summary>
        public DateTime ScheduleDate { get; set; }
        #endregion

        /// <summary>
        /// 使用到的代理
        /// </summary>
        public List<string>? ProxyIds { get; set; } = [];

        #region 临时数据，不保存到数据库

        /// <summary>
        /// smtp 密码解密 key
        /// </summary>
        public List<string> SmtpPasswordSecretKeys { get; set; } = [];

        /// <summary>
        /// 批量发送
        /// </summary>
        public bool SendBatch { get; set; }
        #endregion

        public async Task<SendingGroup> ConvertToSendingGroup(SqlContext db)
        {
            var sendingGroup = new SendingGroup
            {
                // 主题
                Subjects = string.Join("\n", Subjects)
            };

            // 模板
            if (TemplateIds != null && TemplateIds.Count > 0)
            {
                var templates = await db
                    .EmailTemplates.Where(x => TemplateIds.Contains(x.ObjectId))
                    .ToListAsync();
                sendingGroup.Templates = templates;
            }

            // 正文内容
            sendingGroup.Body = Body;

            // 发件箱
            if (SenderAccountIds != null && SenderAccountIds.Count > 0)
            {
                var senderAccounts = await db
                    .SenderAccounts.Where(x => SenderAccountIds.Contains(x.ObjectId))
                    .ToListAsync();
                sendingGroup.SenderAccounts = senderAccounts;
            }

            // 发件箱组
            if (SenderAccountGroupIds != null && SenderAccountGroupIds.Count > 0)
            {
                var senderAccountGroups = await db
                    .EmailGroups.Where(x => SenderAccountGroupIds.Contains(x.ObjectId))
                    .ToListAsync();
                sendingGroup.SenderAccountGroups = senderAccountGroups
                    .Select(x => new IdAndName()
                    {
                        Id = x.Id,
                        ObjectId = x.ObjectId,
                        Name = x.Name,
                        Description = x.Description
                    })
                    .ToList();
            }

            // 收件箱
            if (RecipientEmails != null && RecipientEmails.Count > 0)
            {
                sendingGroup.Recipients = RecipientEmails
                    .Select(email => new EmailAddress { Email = email })
                    .ToList();
            }

            // 收件箱组
            if (RecipientContactGroupIds != null && RecipientContactGroupIds.Count > 0)
            {
                var recipientContactGroups = await db
                    .EmailGroups.Where(x => RecipientContactGroupIds.Contains(x.ObjectId))
                    .ToListAsync();
                sendingGroup.RecipientContactGroups = recipientContactGroups
                    .Select(x => new IdAndName()
                    {
                        Id = x.Id,
                        ObjectId = x.ObjectId,
                        Name = x.Name,
                        Description = x.Description
                    })
                    .ToList();
            }

            // 抄送箱
            if (CcBoxes != null && CcBoxes.Count > 0)
            {
                sendingGroup.CcBoxes = CcBoxes
                    .Select(email => new EmailAddress { Email = email })
                    .ToList();
            }

            // 密送
            if (BccBoxes != null && BccBoxes.Count > 0)
            {
                sendingGroup.BccBoxes = BccBoxes
                    .Select(email => new EmailAddress { Email = email })
                    .ToList();
            }

            // 附件
            if (AttachmentIds != null && AttachmentIds.Count > 0)
            {
                var attachments = await db
                    .FileUsages.Where(x => AttachmentIds.Contains(x.ObjectId))
                    .ToListAsync();
                sendingGroup.Attachments = attachments;
            }

            // 数据
            sendingGroup.Data = Data;

            // 发件类型
            sendingGroup.SendingType = SendingType;
            // 定时发件时间
            sendingGroup.ScheduleDate = ScheduleDate;

            // 代理
            if (ProxyIds != null && ProxyIds.Count > 0)
            {
                var proxies = await db
                    .Proxies.Where(x => ProxyIds.Contains(x.ObjectId))
                    .ToListAsync();
                sendingGroup.ProxyIds = proxies.Select(x => x.Id).ToList();
            }

            // 批量发送
            sendingGroup.SendBatch = SendBatch;

            return sendingGroup;
        }
    }
}
