using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Uamazing.Utils.Web.Token;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Organization;
using UzonMail.ProPlugin.Services.License;
using UzonMail.ProPlugin.Utils;
using UzonMail.Utils.Web.Token;

namespace UzonMail.ProPlugin.Services.Token
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
