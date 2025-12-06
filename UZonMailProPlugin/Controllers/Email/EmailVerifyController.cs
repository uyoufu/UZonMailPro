using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.CorePlugin.Database.Validators;
using UZonMail.CorePlugin.Services.Settings;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.DB.Utils;
using UZonMail.Utils.Web.ResponseModel;
using UZonMail.ProPlugin.Controllers.Base;
using UZonMail.ProPlugin.Services.EmailVerify;

namespace UZonMail.ProPlugin.Controllers.Email
{
    public class EmailVerifyController(
        SqlContext db,
        InboxVerifyService inboxVerify,
        TokenService tokenService
    ) : ControllerBasePro
    {
        /// <summary>
        /// 验证所有无效的邮箱
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpPut("groups/{groupId:long}/verify-invalid-inboxes")]
        public async Task<ResponseResult<bool>> VerifyAllInvalidInboxInGroup(long groupId)
        {
            // 判断是否属于自己的组
            var userId = tokenService.GetUserSqlId();
            var inboxes = db
                .Inboxes.AsNoTracking()
                .Where(x =>
                    x.EmailGroupId == groupId && x.UserId == userId /*&& x.Status != InboxStatus.Valid*/
                );

            await inboxVerify.Validate(userId, new QueryPaginator<Inbox>(inboxes));
            return true.ToSuccessResponse();
        }
    }
}
