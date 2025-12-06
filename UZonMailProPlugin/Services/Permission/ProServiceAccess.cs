using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Utils.Web.Access;
using UZonMail.ProPlugin.Services.License;

namespace UZonMail.ProPlugin.Services.Permission
{
    /// <summary>
    /// 主系统在需要时，会自动调用此服务，生成用户的 pro 权限码
    /// </summary>
    public class ProServiceAccess(LicenseAccessService functionAccess) : IAccessBuilder
    {
        /// <summary>
        /// 生成用户的 pro 权限码
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<Dictionary<long, List<string>>> GenerateUserPermissionCodes(List<long> userIds)
        {

            var access = new HashSet<string>();

            // 隐藏赞助商
            if (await functionAccess.HasProLicense())
            {
                access.Add("hideSponsor");
                access.Add("professional");
            }

            // 企业版本
            if (functionAccess.LicenseInfo.LicenseType.HasFlag(LicenseType.Enterprise))
            {
                access.Add("enterprise");
            }

            // 转换成用户的授权码
            return userIds.ToDictionary(x => x, _ => access.ToList());
        }
    }
}
