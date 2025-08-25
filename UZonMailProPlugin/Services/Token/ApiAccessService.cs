using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UZonMail.Core.Services.UserInfos;
using UZonMail.DB.SQL;
using UZonMail.Utils.Web.Exceptions;
using UZonMail.Utils.Web.Service;
using UZonMailProPlugin.Utils;

namespace UZonMailProPlugin.Services.Token
{
    /// <summary>
    /// API 访问服务
    /// </summary>
    public class ApiAccessService(SqlContext db, UserService userService) : IScopedService
    {
        /// <summary>
        /// 生成 API 访问令牌
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="jwtId"></param>
        /// <param name="expireDate"></param>
        /// <returns></returns>
        public async Task<string> GenerateApiAccessToken(long userId, string jwtId, DateTime expireDate)
        {
            var payloads = new List<Claim>()
            {
                new(JwtRegisteredClaimNames.Jti, jwtId),
                new(ClaimTypes.Role, ApiRoles.ApiUser)
            };

            var userInfo = db.Users.Where(x => x.Id == userId).First();
            var expireTimeSpan = expireDate - DateTime.UtcNow;
            var apiToken = await userService.GenerateToken(userInfo, expireDate, payloads);
            return apiToken;
        }
    }
}
