using UZonMailProPlugin.SQL.ApiAccess;

namespace UZonMailProPlugin.Controllers.ApiAccess.Model
{
    /// <summary>
    /// 生成的访问令牌结果
    /// </summary>
    public class AccessTokenResult: AccessToken
    {
        public string Token { get; set; }

        public AccessTokenResult(AccessToken accessToken, string token)
        {
            Id = accessToken.Id;
            UserId = accessToken.UserId;
            Name = accessToken.Name;
            Description = accessToken.Description;
            Enable = accessToken.Enable;
            ExpireDate = accessToken.ExpireDate;
            Token = token;
        }
    }
}
