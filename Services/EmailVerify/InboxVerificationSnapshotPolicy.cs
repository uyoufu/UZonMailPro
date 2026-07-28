using UzonMail.CorePlugin.Services.EmailVerification;
using UzonMail.ProPlugin.SQL.EmailVerify;

namespace UzonMail.ProPlugin.Services.EmailVerify;

/// <summary>
/// 管理收件箱验证快照的复用与有效期策略
/// </summary>
public static class InboxVerificationSnapshotPolicy
{
    private const string SpfCheckFailedMarker = "SPF check failed";
    private static readonly HashSet<string> LongLivedDomains =
        new(StringComparer.OrdinalIgnoreCase) { "gmail.com", "qq.com" };

    /// <summary>
    /// 判断现有快照是否可以直接复用
    /// </summary>
    public static bool CanReuse(InboxVerificationSnapshot snapshot, DateTime utcNow)
    {
        return snapshot.ExpiresAtUtc > utcNow && !IsLegacySpfFailure(snapshot);
    }

    /// <summary>
    /// 计算本次验证结果的快照到期时间
    /// </summary>
    public static DateTime GetExpiresAtUtc(
        string domain,
        InboxVerificationState state,
        InboxVerificationOptions options,
        DateTime utcNow
    )
    {
        if (state == InboxVerificationState.Unknown)
            return utcNow + options.UnknownSnapshotLifetime;

        var lifetime = IsLongLivedDomain(domain)
            ? options.LongLivedDomainSnapshotLifetime
            : options.DefaultSnapshotLifetime;
        return utcNow + lifetime;
    }

    private static bool IsLegacySpfFailure(InboxVerificationSnapshot snapshot)
    {
        return snapshot.State == InboxVerificationState.Invalid
            && snapshot.FailureReason?.Contains(
                SpfCheckFailedMarker,
                StringComparison.OrdinalIgnoreCase
            ) == true;
    }

    private static bool IsLongLivedDomain(string domain)
    {
        return LongLivedDomains.Contains(domain);
    }
}
