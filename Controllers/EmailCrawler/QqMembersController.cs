using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.ProPlugin.Controllers.Base;
using UzonMail.ProPlugin.Controllers.EmailCrawler.DTOs;
using UzonMail.ProPlugin.SQL;
using UzonMail.Utils.Web.ResponseModel;

namespace UzonMail.ProPlugin.Controllers.EmailCrawler
{
    /// <summary>
    /// QQ 成员
    /// </summary>
    /// <param name="db"></param>
    /// <param name="tokenService"></param>
    public class QqMembersController(SqlContext db, TokenService tokenService) : ControllerBasePro
    {
        /// <summary>
        /// 保存 QQ 群成员为收件联系人
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPut("as-recipient-contacts")]
        public async Task<ResponseResult<bool>> SaveQQMembersAsRecipientContacts(
            [FromBody] QQMemberSaveData data
        )
        {
            var userId = tokenService.GetUserSqlId();

            // 判断是否存在
            var existGroup = await db
                .EmailGroups.Where(x =>
                    x.UserId == userId
                    && x.Category == EmailGroupCategory.Recipient
                    && x.Extra == data.Group.GroupId.ToString()
                )
                .FirstOrDefaultAsync();
            // 如果没找到，则新建
            if (existGroup == null)
            {
                existGroup = new EmailGroup
                {
                    UserId = userId,
                    Category = EmailGroupCategory.Recipient,
                    Name = data.Group.GroupName,
                    Extra = data.Group.GroupId.ToString(),
                    IsDefault = false,
                };
                db.EmailGroups.Add(existGroup);
                await db.SaveChangesAsync();
            }

            // 获取已经存在邮件列表
            var existEmails = await db
                .RecipientContacts.Where(x => x.UserId == userId && x.EmailGroupId == existGroup.Id)
                .Select(x => x.Email)
                .ToListAsync();

            // 去重
            var existEmailsSet = existEmails.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var newQQ = data
                .Users.Where(x => !existEmailsSet.Contains($"{x.UserId}@qq.com"))
                .ToList();
            // 添加新成员
            foreach (var member in newQQ)
            {
                var recipientContact = new RecipientContact
                {
                    UserId = userId,
                    OrganizationId = tokenService.GetOrganizationId(),
                    EmailGroupId = existGroup.Id,
                    Email = $"{member.UserId}@qq.com",
                    Name = member.Nickname
                };
                db.RecipientContacts.Add(recipientContact);
            }
            await db.SaveChangesAsync();

            return true.ToSuccessResponse();
        }
    }
}
