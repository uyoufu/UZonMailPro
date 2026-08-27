namespace UzonMail.ProPlugin.Controllers.Email.DTOs;

/// <summary>
/// 指定收件联系人验证请求。
/// </summary>
public sealed class VerifyRecipientContactsRequest
{
    /// <summary>
    /// 需要验证的收件联系人标识。
    /// </summary>
    public List<long> RecipientContactIds { get; set; } = [];
}
