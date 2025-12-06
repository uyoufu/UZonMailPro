using UZonMail.DB.SQL.Base;

namespace UZonMail.ProPlugin.SQL.ApiAccess
{
    /// <summary>
    /// AccessToken 类表示 API 访问令牌
    /// </summary>
    public class AccessToken : BaseUserId
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// JWT ID
        /// </summary>
        public string JwtId { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime ExpireDate { get; set; }

        /// <summary>
        /// 是否启用
        /// 可以通过设置这个值来控制令牌的有效性
        /// </summary>
        public bool Enable { get; set; } = true;

        /// <summary>
        /// 获取黑名单 Key
        /// </summary>
        /// <returns></returns>
        public string GetBlacklistKey()
        {
            return $"jwt:blacklist:{JwtId}";
        }

        public void InitJwtId()
        {
            JwtId = Sigin.ObjectId.ObjectId.NewObjectId().ToString();
        }
    }
}
