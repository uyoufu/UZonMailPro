using System.Net;
using Microsoft.AspNetCore.Mvc;
using Uamazing.Utils.Web.ResponseModel;
using UzonMail.CorePlugin.Services.EmailVerification;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.ProPlugin.Controllers.Base;
using UzonMail.ProPlugin.Controllers.Email.DTOs;
using UzonMail.ProPlugin.Services.License;
using UzonMail.Utils.Web.ResponseModel;

namespace UzonMail.ProPlugin.Controllers.Email
{
    public class EmailVerifyController(
        InboxVerificationService inboxVerificationService,
        LicenseAccessService licenseAccessService,
        TokenService tokenService
    ) : ControllerBasePro
    {
        /// <summary>
        /// 验证所有无效的邮箱
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        [HttpPut("groups/{groupId:long}/verify")]
        [HttpPut("groups/{groupId:long}/verify-invalid-inboxes")]
        public async Task<ResponseResult<InboxVerificationBatchSummary>> VerifyInboxGroup(
            long groupId
        )
        {
            if (!await licenseAccessService.HasProLicense())
            {
                return ResponseResult<InboxVerificationBatchSummary>.Fail(
                    "当前功能仅专业版及以上版本可用",
                    HttpStatusCode.Unauthorized
                );
            }

            var userId = tokenService.GetUserSqlId();
            var result = await inboxVerificationService.VerifyGroupAsync(userId, groupId);
            return result.ToSuccessResponse();
        }

        /// <summary>
        /// 验证指定收件箱。
        /// </summary>
        [HttpPut("inboxes/verify")]
        public async Task<ResponseResult<InboxVerificationBatchSummary>> VerifyInboxes(
            [FromBody] VerifyInboxesRequest request
        )
        {
            if (!await licenseAccessService.HasProLicense())
            {
                return ResponseResult<InboxVerificationBatchSummary>.Fail(
                    "当前功能仅专业版及以上版本可用",
                    HttpStatusCode.Unauthorized
                );
            }

            var userId = tokenService.GetUserSqlId();
            var result = await inboxVerificationService.VerifyInboxesAsync(
                userId,
                request.InboxIds
            );
            return result.ToSuccessResponse();
        }
    }
}
