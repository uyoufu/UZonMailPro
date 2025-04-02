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
        /// 获取设备ID
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("device-id")]
        public async Task<ResponseResult<string>> GetDeiviceId()
        {
            var deviceId = licenseManager.GetDeviceId();
            return deviceId.ToSuccessResponse();
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
        /// 删除授权
        /// </summary>
        /// <param name="licenseCode"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete()]
        public async Task<ResponseResult<LicenseInfo>> RemoveLicenseInfo()
        {            
            // 从服务器请求授权信息
            var result = await licenseManager.RemoveLicense();
            return result.ToSuccessResponse();
        }

        /// <summary>
        /// 更新现有授权信息
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("exist")]
        public async Task<ResponseResult<LicenseInfo>> UpdateExistingLicenseInfo()
        {
            var result = await licenseManager.UpdateExistingLicense();
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="licenseCode"></param>
        /// <returns></returns>
        [HttpGet()]
        public async Task<ResponseResult<LicenseInfo>> GetLicenseInfo(bool updateFirst)
        {
            var result = await licenseManager.GetLicenseInfo();
            return result.ToSuccessResponse();
        }
    }
}
