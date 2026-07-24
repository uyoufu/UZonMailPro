using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Uamazing.Utils.Web.ResponseModel;
using UzonMail.CorePlugin.Services.SendCore.Runtime;
using UzonMail.Pro.Controllers.SystemInfo.Model;
using UzonMail.ProPlugin.Config;
using UzonMail.ProPlugin.Controllers.Base;
using UzonMail.ProPlugin.Services.License;
using UzonMail.Utils.Web.ResponseModel;

namespace UzonMail.Pro.Controllers.SystemInfo
{
    public class SystemInfoController(
        ISendRuntimeDiagnostics runtimeDiagnostics,
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
        public async Task<ResponseResult<SystemOptions>> GetSystemConfig()
        {
            // 判断是否有企业版本授权
            var enterpriseAccess = await licenseAccess.HasEnterpriseLicense();

            if (!enterpriseAccess)
                return SystemOptions.DefaultSystemConfig().ToSuccessResponse();

            return SystemOptions.GetSystemConfig(configuration).ToSuccessResponse();
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
            await usageInfo.GatherInfomations(runtimeDiagnostics);
            return usageInfo.ToSuccessResponse();
        }
    }
}
