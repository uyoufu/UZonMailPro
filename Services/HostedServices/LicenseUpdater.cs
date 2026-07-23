using UZonMail.CorePlugin.Services.HostedServices;
using UZonMail.ProPlugin.Services.License;

namespace UZonMail.ProPlugin.Services.HostedServices
{
    /// <summary>
    /// 更新授权信息
    /// </summary>
    /// <param name="licenseManager"></param>
    public class LicenseUpdater(LicenseManagerService licenseManager) : IScopedServiceAfterStarting
    {
        public int Order => 0;

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await licenseManager.UpdateExistingLicense();
        }
    }
}
