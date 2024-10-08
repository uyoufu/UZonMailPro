using UZonMail.Utils.Web.ResponseModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UZonMailProPlugin.Controllers.Base;
using UZonMailProPlugin.Utils;
using Uamazing.Utils.Web.ResponseModel;
using UZonMailProPlugin.Services.License;

namespace UZonMail.Pro.Controllers.License
{
    /// <summary>
    /// 授权管理
    /// </summary>
    public class LicenseController(LicenseManagerService licenseManager) : ControllerBasePro
    {
        /// <summary>
        /// 获取商业的授权的权限码
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("access")]
        public async Task<ResponseResult<List<string>>> GetAccess(string userId)
        {
            var licensInfo = await licenseManager.GetLicenseInfo();
            var access = new HashSet<string>();

            if(licensInfo.LicenseType.HasFlag(LicenseType.Professional)|| licensInfo.LicenseType.HasFlag(LicenseType.Professional))
            {
                access.Add("hideSponsor");
            }

            // 如果是商业版本，返回商业的授权数据
            if (licensInfo.LicenseType.HasFlag(LicenseType.Professional))
            {
                access.Add("professional");
            }

            if (licensInfo.LicenseType.HasFlag(LicenseType.Enterprise))
            {
                access.Add("enterprise");
            }

            return access.ToList().ToSuccessResponse();
        }

        /// <summary>
        /// 更新授权
        /// </summary>
        /// <param name="license"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPut()]
        public async Task<ResponseResult<LicenseInfo>> UpdateLicenseInfo(string licenseCode)
        {
            if (string.IsNullOrWhiteSpace(licenseCode))
            {
                return ResponseResult<LicenseInfo>.Fail("授权码不能为空");
            }

            // 从服务器请求授权信息
            var result = await licenseManager.UpdateLicense(licenseCode);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="licenseCode"></param>
        /// <returns></returns>
        [HttpGet()]
        public async Task<ResponseResult<LicenseInfo>> GetLicenseInfo()
        {
            var result = await licenseManager.GetLicenseInfo();
            return result.ToSuccessResponse();
        }
    }
}
