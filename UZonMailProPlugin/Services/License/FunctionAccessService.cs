using UZonMail.Utils.Web.Service;

namespace UZonMailProPlugin.Services.License
{
    /// <summary>
    /// 功能权限服务
    /// </summary>
    public class FunctionAccessService(LicenseManagerService licenseManager) : IScopedService
    {
        /// <summary>
        /// 是否存在专业授权
        /// 企业版本也包含专业版本
        /// </summary>
        /// <returns></returns>
        public async Task<bool> HasProLicense(LicenseInfo licenseInfo = null)
        {
            licenseInfo ??= await licenseManager.GetLicenseInfo();
            return licenseInfo.LicenseType >= LicenseType.Professional;
        }

        /// <summary>
        /// 是否存在企业授权
        /// </summary>
        /// <returns></returns>
        public async Task<bool> HasEnterpriseLicense(LicenseInfo licenseInfo = null)
        {
            licenseInfo ??= await licenseManager.GetLicenseInfo();
            return licenseInfo.LicenseType.HasFlag(LicenseType.Enterprise);
        }

        /// <summary>
        /// 是否有邮件跟踪的权限
        /// </summary>
        /// <param name="licenseInfo"></param>
        /// <returns></returns>
        public async Task<bool> HasEmailTrackingAccess(LicenseInfo licenseInfo = null)
        {
            return await HasEnterpriseLicense(licenseInfo);
        }

        /// <summary>
        /// 是否有邮件跟踪的权限
        /// </summary>
        /// <param name="licenseInfo"></param>
        /// <returns></returns>
        public async Task<bool> HasUnsubscribeAccess(LicenseInfo licenseInfo = null)
        {
            return await HasEnterpriseLicense(licenseInfo);
        }

        /// <summary>
        /// 是否有Tiktok邮箱爬虫的权限
        /// </summary>
        /// <param name="licenseInfo"></param>
        /// <returns></returns>
        public async Task<bool> HasTiktokEmailCrawlerAccess(LicenseInfo licenseInfo = null)
        {
            return await HasEnterpriseLicense(licenseInfo);
        }
    }
}
