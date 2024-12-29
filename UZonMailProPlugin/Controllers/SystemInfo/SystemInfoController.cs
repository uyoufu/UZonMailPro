using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UZonMail.Utils.Web.ResponseModel;
using UZonMailProPlugin.Controllers.Base;
using UZonMail.Pro.Controllers.SystemInfo.Model;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Core.Services.SendCore.WaitList;
using UZonMail.Core.Services.SendCore.Outboxes;
using UZonMail.Core.Services.SendCore;

namespace UZonMail.Pro.Controllers.SystemInfo
{
    public class SystemInfoController(GroupTasksList groupTasksList
        , OutboxesPoolList outboxesPools
        , SendingThreadsManager sendingThreadsManager) : ControllerBasePro
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
            await usageInfo.GatherInfomations(groupTasksList, outboxesPools, sendingThreadsManager);
            return usageInfo.ToSuccessResponse();
        }
    }
}
