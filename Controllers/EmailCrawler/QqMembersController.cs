using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.CorePlugin.Services.Settings;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.ProPlugin.Controllers.Base;
using UZonMail.ProPlugin.Controllers.EmailCrawler.DTOs;
using UZonMail.ProPlugin.SQL;
using UZonMail.Utils.Web.ResponseModel;

namespace UZonMail.ProPlugin.Controllers.EmailCrawler
{
    /// <summary>
    /// QQ 成员
    /// </summary>
    /// <param name="db"></param>
    /// <param name="tokenService"></param>
    public class QqMembersController(SqlContext db, TokenService tokenService) : ControllerBasePro
    {
        /// <summary>
        /// 保存 QQ 群成员为收件箱
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPut("as-inbox")]
        public async Task<ResponseResult<bool>> SaveQQMemberAsInbox(
            [FromBody] QQMemberSaveData data
        )
        {
            var userId = tokenService.GetUserSqlId();

            // 判断是否存在
            var existGroup = await db
                .EmailGroups.Where(x =>
                    x.UserId == userId
                    && x.Type == EmailGroupType.InBox
                    && x.Extra == data.Group.GroupId.ToString()
                )
                .FirstOrDefaultAsync();
            // 如果没找到，则新建
            if (existGroup == null)
            {
                existGroup = new EmailGroup
                {
                    UserId = userId,
                    Type = EmailGroupType.InBox,
                    Name = data.Group.GroupName,
                    Extra = data.Group.GroupId.ToString(),
                    IsDefault = false,
                };
                db.EmailGroups.Add(existGroup);
                await db.SaveChangesAsync();
            }

            // 获取已经存在邮件列表
            var existEmails = await db
                .Inboxes.Where(x => x.UserId == userId && x.EmailGroupId == existGroup.Id)
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
                var inbox = new Inbox
                {
                    UserId = userId,
                    EmailGroupId = existGroup.Id,
                    Email = $"{member.UserId}@qq.com",
                    Name = member.Nickname
                };
                db.Inboxes.Add(inbox);
            }
            await db.SaveChangesAsync();

            return true.ToSuccessResponse();
        }
    }
}
