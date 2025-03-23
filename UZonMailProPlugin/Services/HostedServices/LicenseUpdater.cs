using UZonMail.Core.Services.HostedServices;
using UZonMail.Utils.Web.Service;
using UZonMailProPlugin.Services.License;

namespace UZonMailProPlugin.Services.HostedServices
{
    /// <summary>
    /// 更新授权信息
    /// </summary>
    /// <param name="licenseManager"></param>
    public class LicenseUpdater(LicenseManagerService licenseManager) : IHostedServiceStart
    {
        public int Order => 0;

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await licenseManager.GetLicenseInfo();
        }
    }
}
