using UzonMail.Utils.Web.Configs;

namespace UzonMail.ProPlugin.Services.EmailVerify;

/// <summary>
/// 收件箱验证快照与网络探测配置。
/// </summary>
[OptionName("InboxVerification")]
public sealed class InboxVerificationOptions : IAppOptions
{
    public TimeSpan DefaultSnapshotLifetime { get; set; } = TimeSpan.FromDays(30);

    public TimeSpan LongLivedDomainSnapshotLifetime { get; set; } = TimeSpan.FromDays(365);

    public TimeSpan MxCacheLifetime { get; set; } = TimeSpan.FromHours(24);

    public TimeSpan SmtpTimeout { get; set; } = TimeSpan.FromSeconds(10);

    public int VerificationConcurrency { get; set; } = 16;
}
