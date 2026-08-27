using Microsoft.EntityFrameworkCore;
using UzonMail.CorePlugin.Services.EmailVerification;
using UzonMail.DB.SQL.Base;

namespace UzonMail.ProPlugin.SQL.EmailVerify;

/// <summary>
/// 按邮箱共享的持久化验证快照
/// </summary>
[Index(nameof(NormalizedEmail), IsUnique = true)]
public sealed class RecipientContactVerificationSnapshot : SqlId
{
    public string NormalizedEmail { get; set; } = string.Empty;

    public RecipientContactVerificationState State { get; set; }

    public string? FailureReason { get; set; }

    public DateTime VerifiedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public string? SyntaxDomain { get; set; }

    public string? SyntaxUsername { get; set; }

    public string? SyntaxSuggestion { get; set; }

    public bool IsValidSyntax { get; set; }

    public bool IsDisposable { get; set; }

    public bool IsRoleAccount { get; set; }

    public bool IsB2C { get; set; }

    public long? MxDomainCacheId { get; set; }

    public MxDomainCache? MxDomainCache { get; set; }

    public bool CanConnectSmtp { get; set; }

    public bool HasFullRecipientContact { get; set; }

    public bool IsCatchAll { get; set; }

    public bool IsDeliverable { get; set; }

    public bool IsDisabled { get; set; }
}
