namespace UzonMail.ProPlugin.Controllers.Email.DTOs;

/// <summary>
/// 指定收件箱验证请求。
/// </summary>
public sealed class VerifyInboxesRequest
{
    /// <summary>
    /// 需要验证的收件箱标识。
    /// </summary>
    public List<long> InboxIds { get; set; } = [];
}
