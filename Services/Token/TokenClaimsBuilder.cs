using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Uamazing.Utils.Web.Token;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Organization;
using UZonMail.ProPlugin.Services.License;
using UZonMail.ProPlugin.Utils;
using UZonMail.Utils.Web.Token;

namespace UZonMail.ProPlugin.Services.Token
{
    /// <summary>
    /// TokenClains 创建器
    /// </summary>
    /// <param name="licenseManager"></param>
    public class TokenClaimsBuilder(LicenseManagerService licenseManager) : ITokenClaimBuilder
    {
        public async Task<List<Claim>> Build(ITokenSource tokenSource)
        {
            var license = await licenseManager.GetLicenseInfo();
            var userInfo = tokenSource;

            var claims = new List<Claim>();
            // 专业版本
            if (license.LicenseType == LicenseType.Professional)
            {
                claims.Add(new Claim(ClaimTypes.Role, ApiRoles.Professional));

                // 专业版本管理员
                if (userInfo.IsSuperAdmin)
                {
                    claims.Add(new Claim(ClaimTypes.Role, ApiRoles.ProfessionalAdmin));
                }
            }

            // 企业版本
            if (license.LicenseType == LicenseType.Enterprise)
            {
                claims.Add(new Claim(ClaimTypes.Role, ApiRoles.Enterprise));

                // 企业版本管理员
                if (userInfo.IsSuperAdmin)
                {
                    claims.Add(new Claim(ClaimTypes.Role, ApiRoles.EnterpriseAdmin));
                }
            }

            return claims;
        }
    }
}
