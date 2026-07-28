using MailKit.Net.Smtp;
using UzonMail.CorePlugin.Services.EmailVerification;

namespace UzonMail.ProPlugin.Services.EmailVerify;

/// <summary>
/// SMTP 收件箱探测停止时所在的命令阶段
/// </summary>
public enum SmtpProbeStage
{
    Helo,
    MailFrom,
    Recipient,
}

/// <summary>
/// SMTP 收件箱探测的命令阶段与服务器响应
/// </summary>
public sealed record SmtpProbeResult(SmtpProbeStage Stage, SmtpResponse Response);

/// <summary>
/// 将 SMTP 探测结果转换为收件箱验证证据
/// </summary>
public static class SmtpVerificationEvidenceClassifier
{
    /// <summary>
    /// 根据探测停止阶段和响应状态生成验证结论
    /// </summary>
    public static SmtpVerificationEvidence Classify(SmtpProbeResult probeResult)
    {
        // 收件服务商可能因发件域 SPF 或反滥用策略在 RCPT TO 前拒绝，尚未评估目标地址是否存在
        if (probeResult.Stage != SmtpProbeStage.Recipient)
            return new SmtpVerificationEvidence(
                InboxVerificationState.Unknown,
                probeResult.Response.Response,
                true,
                false,
                false,
                false,
                false
            );

        var statusCode = (int)probeResult.Response.StatusCode;
        if (statusCode is 550 or 551 or 553)
            return new SmtpVerificationEvidence(
                InboxVerificationState.Invalid,
                probeResult.Response.Response,
                true,
                false,
                false,
                false,
                true
            );
        if (statusCode == 552)
            return new SmtpVerificationEvidence(
                InboxVerificationState.Unknown,
                probeResult.Response.Response,
                true,
                true,
                false,
                false,
                false
            );
        if (statusCode is < 200 or >= 300)
            return new SmtpVerificationEvidence(
                InboxVerificationState.Unknown,
                probeResult.Response.Response,
                true,
                false,
                false,
                false,
                false
            );

        return new SmtpVerificationEvidence(
            InboxVerificationState.Valid,
            null,
            true,
            false,
            false,
            true,
            false
        );
    }

    /// <summary>
    /// 判断服务器是否在收件人阶段接受了地址
    /// </summary>
    public static bool IsRecipientAccepted(SmtpProbeResult probeResult)
    {
        var statusCode = (int)probeResult.Response.StatusCode;
        return probeResult.Stage == SmtpProbeStage.Recipient && statusCode is >= 200 and < 300;
    }
}
