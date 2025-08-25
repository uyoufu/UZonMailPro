using UZonMail.DB.SQL.Base;

namespace UZonMailProPlugin.Services.License
{
    /// <summary>
    /// 授权信息
    /// </summary>
    public class LicenseInfo : SqlId
    {
        /// <summary>
        /// 授权码
        /// </summary>
        public string LicenseKey { get; set; }

        /// <summary>
        /// 激活时间
        /// </summary>
        public DateTime ActiveDate { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime ExpireDate { get; set; }

        /// <summary>
        /// 最近一次更新授权的日期
        /// </summary>
        public DateTime LastUpdateDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 授权类型
        /// </summary>
        public LicenseType LicenseType { get; set; }

        /// <summary>
        /// 创建企业版授权
        /// </summary>
        /// <returns></returns>
        public static LicenseInfo CreateEnterpriseLicense()
        {
            return new LicenseInfo()
            {
                LicenseType = LicenseType.Enterprise,
                ExpireDate = DateTime.UtcNow.AddYears(99),
                ActiveDate = DateTime.UtcNow
            };
        }

        /// <summary>
        /// 创建默认的授权
        /// </summary>
        /// <returns></returns>
        public static LicenseInfo CreateDefaultLicense()
        {
            return new LicenseInfo()
            {
                LicenseType = LicenseType.Community,
                ExpireDate = DateTime.UtcNow.AddYears(99),
                ActiveDate = DateTime.UtcNow
            };
        }
    }
}
