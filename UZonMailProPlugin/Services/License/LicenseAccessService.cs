using UZonMail.Utils.Web.Service;

namespace UZonMailProPlugin.Services.License
{
    /// <summary>
    /// 功能权限服务
    /// </summary>
    public class LicenseAccessService : ISingletonService
    {
        /// <summary>
        /// 授权信息
        /// </summary>
        public LicenseInfo LicenseInfo { get; set; } = LicenseInfo.CreateDefaultLicense();

        /// <summary>
        /// 是否存在专业授权
        /// 企业版本也包含专业版本
        /// </summary>
        /// <returns></returns>
        public async Task<bool> HasProLicense()
        {
            return LicenseInfo.LicenseType >= LicenseType.Professional;
        }

        /// <summary>
        /// 是否存在企业授权
        /// </summary>
        /// <returns></returns>
        public async Task<bool> HasEnterpriseLicense()
        {
            return LicenseInfo.LicenseType.HasFlag(LicenseType.Enterprise);
        }

        /// <summary>
        /// 是否有邮件跟踪的权限
        /// </summary>
        /// <param name="licenseInfo"></param>
        /// <returns></returns>
        public async Task<bool> HasEmailTrackingAccess()
        {
            return await HasEnterpriseLicense();
        }

        /// <summary>
        /// 是否有邮件跟踪的权限
        /// </summary>
        /// <param name="licenseInfo"></param>
        /// <returns></returns>
        public async Task<bool> HasUnsubscribeAccess()
        {
            return await HasEnterpriseLicense();
        }

        /// <summary>
        /// 是否有Tiktok邮箱爬虫的权限
        /// </summary>
        /// <param name="licenseInfo"></param>
        /// <returns></returns>
        public async Task<bool> HasTiktokEmailCrawlerAccess()
        {
            return await HasEnterpriseLicense();
        }

        /// <summary>
        /// 是否有动态代理的权限
        /// </summary>
        /// <param name="licenseInfo"></param>
        /// <returns></returns>
        public async Task<bool> HasDynamicProxyAccess()
        {
            return await HasProLicense();
        }
    }
}
