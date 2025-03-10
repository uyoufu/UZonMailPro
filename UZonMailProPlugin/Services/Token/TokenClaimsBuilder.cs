using System.Security.Claims;
using Uamazing.Utils.Web.Token;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Organization;
using UZonMail.Utils.Web.Token;
using UZonMailProPlugin.Services.License;
using UZonMailProPlugin.Utils;

namespace UZonMailProPlugin.Services.Token
{
    public class TokenClaimsBuilder(LicenseManagerService licenseManager) : ITokenClaimBuilder
    {
        public async Task<List<Claim>> Build(ITokenSource tokenSource)
        {
            var license = await licenseManager.GetLicenseInfo();
            var userInfo = tokenSource as User;

            var results = new List<Claim>();
            // 专业版本
            if (license.LicenseType == LicenseType.Professional)
            {
                results.Add(new Claim(ClaimTypes.Role, ApiRoles.Professional.ToString()));

                // 专业版本管理员
                if (userInfo.IsSuperAdmin)
                {
                    results.Add(new Claim(ClaimTypes.Role, ApiRoles.ProfessionalAdmin.ToString()));
                }
            }

            // 企业版本
            if (license.LicenseType == LicenseType.Enterprise)
            {
                results.Add(new Claim(ClaimTypes.Role, ApiRoles.Enterprise.ToString()));

                // 企业版本管理员
                if (userInfo.IsSuperAdmin)
                {
                    results.Add(new Claim(ClaimTypes.Role, ApiRoles.EnterpriseAdmin.ToString()));
                }
            }

            return results;
        }
    }
}
