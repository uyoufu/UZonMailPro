using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UZonMail.Utils.Web.ResponseModel;
using UZonMail.Core.Services.EmailSending.OutboxPool;
using UZonMail.Core.Services.EmailSending.Sender;
using UZonMail.Core.Services.EmailSending.WaitList;
using UZonMailProPlugin.Controllers.Base;
using UZonMail.Pro.Controllers.SystemInfo.Model;
using Uamazing.Utils.Web.ResponseModel;

namespace UZonMail.Pro.Controllers.SystemInfo
{
    public class SystemInfoController(UserSendingGroupsManager userSendingGroupsManager
        , UserOutboxesPoolManager userOutboxesPoolManager
        , SendingThreadManager sendingThreadManager) : ControllerBasePro
    {
        /// <summary>
        /// 仅管理员可访问
        /// </summary>
        /// <returns></returns>
        [HttpGet("resource-usage")]
        [AllowAnonymous]
        public async Task<ResponseResult<SystemUsageInfo>> GetSystemResourceUsage()
        {
            var usageInfo = new SystemUsageInfo();
            await usageInfo.GatherInfomations(userSendingGroupsManager, userOutboxesPoolManager, sendingThreadManager);
            return usageInfo.ToSuccessResponse();
        }
    }
}
