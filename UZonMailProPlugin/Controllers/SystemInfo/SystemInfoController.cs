using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.CorePlugin.Services.SendCore;
using UZonMail.CorePlugin.Services.SendCore.Outboxes;
using UZonMail.CorePlugin.Services.SendCore.WaitList;
using UZonMail.Pro.Controllers.SystemInfo.Model;
using UZonMail.Utils.Web.ResponseModel;
using UZonMail.ProPlugin.Config;
using UZonMail.ProPlugin.Controllers.Base;
using UZonMail.ProPlugin.Services.License;

namespace UZonMail.Pro.Controllers.SystemInfo
{
    public class SystemInfoController(
        GroupTasksList groupTasksList,
        OutboxesManager outboxesPools,
        OutboxTasksManager sendingThreadsManager,
        LicenseAccessService licenseAccess,
        IConfiguration configuration
    ) : ControllerBasePro
    {
        /// <summary>
        /// 获取系统配置信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("config")]
        [AllowAnonymous]
        public async Task<ResponseResult<SystemConfig>> GetSystemConfig()
        {
            // 判断是否有企业版本授权
            var enterpriseAccess = await licenseAccess.HasEnterpriseLicense();

            if (!enterpriseAccess)
                return SystemConfig.DefaultSystemConfig().ToSuccessResponse();

            return SystemConfig.GetSystemConfig(configuration).ToSuccessResponse();
        }

        /// <summary>
        /// 仅管理员可访问
        /// </summary>
        /// <returns></returns>
        [HttpGet("resource-usage")]
        [Authorize(Roles = "Admin")]
        public async Task<ResponseResult<SystemUsageInfo>> GetSystemResourceUsage()
        {
            var usageInfo = new SystemUsageInfo();
            await usageInfo.GatherInfomations(groupTasksList, outboxesPools, sendingThreadsManager);
            return usageInfo.ToSuccessResponse();
        }
    }
}
