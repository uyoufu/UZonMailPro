using System.Collections.Concurrent;
using System.Net.Mail;
using DnsClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UzonMail.CorePlugin.Services.EmailVerification;
using UzonMail.ProPlugin.SQL;
using UzonMail.ProPlugin.SQL.EmailVerify;
using UzonMail.Utils.Web.Service;

namespace UzonMail.ProPlugin.Services.EmailVerify;

/// <summary>
/// 语法验证证据。
/// </summary>
public sealed record SyntaxVerificationEvidence(
    InboxVerificationState State,
    string? FailureReason,
    string Domain,
    string Username,
    bool IsValidSyntax,
    string? Suggestion
) : InboxVerificationEvidence(State, FailureReason);

/// <summary>
/// MX 验证证据。
/// </summary>
public sealed record MxVerificationEvidence(
    InboxVerificationState State,
    string? FailureReason,
    bool AcceptsMail,
    IReadOnlyList<string> Records
) : InboxVerificationEvidence(State, FailureReason);

/// <summary>
/// SMTP 验证证据。
/// </summary>
public sealed record SmtpVerificationEvidence(
    InboxVerificationState State,
    string? FailureReason,
    bool CanConnectSmtp,
    bool HasFullInbox,
    bool IsCatchAll,
    bool IsDeliverable,
    bool IsDisabled
) : InboxVerificationEvidence(State, FailureReason);

/// <summary>
/// 收件箱风险属性证据。
/// </summary>
public sealed record MiscVerificationEvidence(
    InboxVerificationState State,
    string? FailureReason,
    bool IsDisposable,
    bool IsRoleAccount,
    bool IsB2C
) : InboxVerificationEvidence(State, FailureReason);

/// <summary>
/// Pro 收件箱验证器，按成本串行执行语法、杂项、MX 与 SMTP 检查。
/// </summary>
public sealed class ProInboxVerificationEvidence(
    SqlContextPro db,
    IOptions<InboxVerificationOptions> options
) : IInboxVerifier, IScopedService<IInboxVerifier>
{
    private static readonly HashSet<string> RoleAccounts =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "admin",
            "contact",
            "hello",
            "info",
            "sales",
            "service",
            "support"
        };
    private static readonly HashSet<string> B2CDomains =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "gmail.com",
            "hotmail.com",
            "outlook.com",
            "qq.com",
            "163.com",
            "126.com",
            "yahoo.com"
        };
    private static readonly HashSet<string> DisposableDomains =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "10minutemail.com",
            "guerrillamail.com",
            "mailinator.com",
            "tempmail.com"
        };
    private static readonly HashSet<string> LongLivedDomains =
        new(StringComparer.OrdinalIgnoreCase) { "gmail.com", "qq.com" };
    private static readonly ConcurrentDictionary<string, CachedMxResult> MemoryMxCache = [];
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> MxLocks = [];
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> SnapshotLocks = [];

    /// <inheritdoc />
    public async Task<InboxVerificationReport> VerifyAsync(
        InboxVerificationRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var snapshotLock = SnapshotLocks.GetOrAdd(normalizedEmail, _ => new SemaphoreSlim(1, 1));
        await snapshotLock.WaitAsync(cancellationToken);
        try
        {
            var snapshot = await db
                .InboxVerificationSnapshots.Include(x => x.MxDomainCache)
                .ThenInclude(x => x!.Records)
                .FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
            if (snapshot?.ExpiresAtUtc > DateTime.UtcNow)
                return ToReport(request, snapshot);

            var syntax = ValidateSyntax(normalizedEmail);
            var misc = CreateMiscEvidence(syntax);
            MxVerificationEvidence mx;
            SmtpVerificationEvidence smtp;

            if (syntax.State != InboxVerificationState.Valid)
            {
                mx = new MxVerificationEvidence(InboxVerificationState.NotChecked, null, false, []);
                smtp = CreateNotCheckedSmtpEvidence();
            }
            else
            {
                mx = await GetMxEvidenceAsync(syntax.Domain, cancellationToken);
                smtp =
                    mx.State == InboxVerificationState.Valid
                        ? await ValidateSmtpAsync(
                            normalizedEmail,
                            syntax.Domain,
                            mx,
                            cancellationToken
                        )
                        : CreateNotCheckedSmtpEvidence();
            }

            var state = MergeState(syntax.State, mx.State, smtp.State);
            var failureReason = new InboxVerificationEvidence[] { syntax, mx, smtp }
                .Select(x => x.FailureReason)
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
            snapshot ??= new InboxVerificationSnapshot { NormalizedEmail = normalizedEmail };
            ApplySnapshot(snapshot, state, failureReason, syntax, misc, mx, smtp);
            if (snapshot.Id == 0)
                db.InboxVerificationSnapshots.Add(snapshot);

            try
            {
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException) when (snapshot.Id == 0)
            {
                // 跨实例并发创建时，以已提交的最新快照为准，避免重复网络探测后暴露唯一键错误
                db.Entry(snapshot).State = EntityState.Detached;
                var persistedSnapshot = await db
                    .InboxVerificationSnapshots.Include(x => x.MxDomainCache)
                    .ThenInclude(x => x!.Records)
                    .FirstOrDefaultAsync(
                        x => x.NormalizedEmail == normalizedEmail,
                        cancellationToken
                    );
                if (persistedSnapshot?.ExpiresAtUtc > DateTime.UtcNow)
                    return ToReport(request, persistedSnapshot);
                throw;
            }

            return ToReport(request, snapshot);
        }
        finally
        {
            snapshotLock.Release();
        }
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static SyntaxVerificationEvidence ValidateSyntax(string email)
    {
        try
        {
            var mailAddress = new MailAddress(email);
            var isValid =
                string.Equals(mailAddress.Address, email, StringComparison.OrdinalIgnoreCase)
                && mailAddress.Host.Contains('.');
            if (!isValid)
                return new SyntaxVerificationEvidence(
                    InboxVerificationState.Invalid,
                    "邮箱格式不正确",
                    string.Empty,
                    string.Empty,
                    false,
                    null
                );

            var domain = mailAddress.Host.ToLowerInvariant();
            var username = mailAddress.User;
            return new SyntaxVerificationEvidence(
                InboxVerificationState.Valid,
                null,
                domain,
                username,
                true,
                GetDomainSuggestion(domain)
            );
        }
        catch (FormatException)
        {
            return new SyntaxVerificationEvidence(
                InboxVerificationState.Invalid,
                "邮箱格式不正确",
                string.Empty,
                string.Empty,
                false,
                null
            );
        }
    }

    private static MiscVerificationEvidence CreateMiscEvidence(SyntaxVerificationEvidence syntax)
    {
        if (syntax.State != InboxVerificationState.Valid)
            return new MiscVerificationEvidence(
                InboxVerificationState.NotChecked,
                null,
                false,
                false,
                false
            );

        return new MiscVerificationEvidence(
            InboxVerificationState.Valid,
            null,
            DisposableDomains.Contains(syntax.Domain),
            RoleAccounts.Contains(syntax.Username),
            B2CDomains.Contains(syntax.Domain)
        );
    }

    private async Task<MxVerificationEvidence> GetMxEvidenceAsync(
        string domain,
        CancellationToken cancellationToken
    )
    {
        if (
            MemoryMxCache.TryGetValue(domain, out var memoryResult)
            && memoryResult.ExpiresAtUtc > DateTime.UtcNow
        )
            return memoryResult.Evidence;

        var domainLock = MxLocks.GetOrAdd(domain, _ => new SemaphoreSlim(1, 1));
        await domainLock.WaitAsync(cancellationToken);
        try
        {
            if (
                MemoryMxCache.TryGetValue(domain, out memoryResult)
                && memoryResult.ExpiresAtUtc > DateTime.UtcNow
            )
                return memoryResult.Evidence;

            var cache = await db
                .MxDomainCaches.Include(x => x.Records)
                .FirstOrDefaultAsync(x => x.Domain == domain, cancellationToken);
            if (cache?.ExpiresAtUtc > DateTime.UtcNow)
            {
                var evidence = ToMxEvidence(cache);
                MemoryMxCache[domain] = new CachedMxResult(evidence, cache.ExpiresAtUtc, cache.Id);
                return evidence;
            }

            try
            {
                var lookup = new LookupClient();
                var response = await lookup.QueryAsync(
                    domain,
                    QueryType.MX,
                    cancellationToken: cancellationToken
                );
                var records = response
                    .Answers.MxRecords()
                    .OrderBy(x => x.Preference)
                    .Select(x => new MxDomainRecord
                    {
                        Host = x.Exchange.Value.TrimEnd('.'),
                        Priority = x.Preference
                    })
                    .ToList();
                cache ??= new MxDomainCache { Domain = domain };
                cache.AcceptsMail = records.Count > 0;
                cache.FailureReason = records.Count == 0 ? "域名不存在 MX 记录" : null;
                cache.CheckedAtUtc = DateTime.UtcNow;
                cache.ExpiresAtUtc = DateTime.UtcNow + options.Value.MxCacheLifetime;
                cache.Records.Clear();
                cache.Records.AddRange(records);
                if (cache.Id == 0)
                    db.MxDomainCaches.Add(cache);
                await db.SaveChangesAsync(cancellationToken);
                var evidence = ToMxEvidence(cache);
                MemoryMxCache[domain] = new CachedMxResult(evidence, cache.ExpiresAtUtc, cache.Id);
                return evidence;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                var evidence = new MxVerificationEvidence(
                    InboxVerificationState.Unknown,
                    "无法查询 MX 记录",
                    false,
                    []
                );
                MemoryMxCache[domain] = new CachedMxResult(
                    evidence,
                    DateTime.UtcNow + TimeSpan.FromMinutes(5),
                    null
                );
                return evidence;
            }
        }
        finally
        {
            domainLock.Release();
        }
    }

    private async Task<SmtpVerificationEvidence> ValidateSmtpAsync(
        string email,
        string domain,
        MxVerificationEvidence mx,
        CancellationToken cancellationToken
    )
    {
        foreach (var record in mx.Records)
        {
            using var client = new VerifySmtpClient
            {
                Timeout = (int)options.Value.SmtpTimeout.TotalMilliseconds
            };
            if (!await client.ConnectToMx(record))
                continue;

            try
            {
                var targetResponse = await client.CheckExist(email, [domain]);
                var targetStatus = (int)targetResponse.StatusCode;
                if (targetStatus is 550 or 551 or 553)
                    return new SmtpVerificationEvidence(
                        InboxVerificationState.Invalid,
                        targetResponse.Response,
                        true,
                        false,
                        false,
                        false,
                        true
                    );
                if (targetStatus == 552)
                    return new SmtpVerificationEvidence(
                        InboxVerificationState.Unknown,
                        targetResponse.Response,
                        true,
                        true,
                        false,
                        false,
                        false
                    );
                if (targetStatus is < 200 or >= 300)
                    return new SmtpVerificationEvidence(
                        InboxVerificationState.Unknown,
                        targetResponse.Response,
                        true,
                        false,
                        false,
                        false,
                        false
                    );

                var randomEmail = $"uzon-verification-{Guid.NewGuid():N}@{domain}";
                var catchAllResponse = await client.CheckExist(randomEmail, [domain]);
                if ((int)catchAllResponse.StatusCode is >= 200 and < 300)
                    return new SmtpVerificationEvidence(
                        InboxVerificationState.Unknown,
                        "目标域为 catch-all",
                        true,
                        false,
                        true,
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
            finally
            {
                if (client.IsConnected)
                    await client.DisconnectAsync(true, cancellationToken);
            }
        }

        return new SmtpVerificationEvidence(
            InboxVerificationState.Unknown,
            "无法连接目标 SMTP 服务器",
            false,
            false,
            false,
            false,
            false
        );
    }

    private static SmtpVerificationEvidence CreateNotCheckedSmtpEvidence() =>
        new(InboxVerificationState.NotChecked, null, false, false, false, false, false);

    private static InboxVerificationState MergeState(params InboxVerificationState[] states)
    {
        if (states.Contains(InboxVerificationState.Invalid))
            return InboxVerificationState.Invalid;
        if (states.All(x => x == InboxVerificationState.Valid))
            return InboxVerificationState.Valid;
        return InboxVerificationState.Unknown;
    }

    private static MxVerificationEvidence ToMxEvidence(MxDomainCache cache) =>
        new(
            cache.AcceptsMail ? InboxVerificationState.Valid : InboxVerificationState.Invalid,
            cache.FailureReason,
            cache.AcceptsMail,
            cache.Records.OrderBy(x => x.Priority).Select(x => x.Host).ToList()
        );

    private void ApplySnapshot(
        InboxVerificationSnapshot snapshot,
        InboxVerificationState state,
        string? failureReason,
        SyntaxVerificationEvidence syntax,
        MiscVerificationEvidence misc,
        MxVerificationEvidence mx,
        SmtpVerificationEvidence smtp
    )
    {
        snapshot.State = state;
        snapshot.FailureReason = failureReason;
        snapshot.VerifiedAtUtc = DateTime.UtcNow;
        snapshot.ExpiresAtUtc = GetSnapshotExpiration(syntax.Domain);
        snapshot.SyntaxDomain = syntax.Domain;
        snapshot.SyntaxUsername = syntax.Username;
        snapshot.SyntaxSuggestion = syntax.Suggestion;
        snapshot.IsValidSyntax = syntax.IsValidSyntax;
        snapshot.IsDisposable = misc.IsDisposable;
        snapshot.IsRoleAccount = misc.IsRoleAccount;
        snapshot.IsB2C = misc.IsB2C;
        snapshot.CanConnectSmtp = smtp.CanConnectSmtp;
        snapshot.HasFullInbox = smtp.HasFullInbox;
        snapshot.IsCatchAll = smtp.IsCatchAll;
        snapshot.IsDeliverable = smtp.IsDeliverable;
        snapshot.IsDisabled = smtp.IsDisabled;
        snapshot.MxDomainCacheId = MemoryMxCache.TryGetValue(syntax.Domain, out var mxCache)
            ? mxCache.DatabaseId
            : null;
    }

    private InboxVerificationReport ToReport(
        InboxVerificationRequest request,
        InboxVerificationSnapshot snapshot
    )
    {
        var mxCache = snapshot.MxDomainCache;
        var syntax = new SyntaxVerificationEvidence(
            snapshot.IsValidSyntax ? InboxVerificationState.Valid : InboxVerificationState.Invalid,
            snapshot.IsValidSyntax ? null : snapshot.FailureReason,
            snapshot.SyntaxDomain ?? string.Empty,
            snapshot.SyntaxUsername ?? string.Empty,
            snapshot.IsValidSyntax,
            snapshot.SyntaxSuggestion
        );
        var misc = new MiscVerificationEvidence(
            InboxVerificationState.Valid,
            null,
            snapshot.IsDisposable,
            snapshot.IsRoleAccount,
            snapshot.IsB2C
        );
        var mx = new MxVerificationEvidence(
            mxCache?.AcceptsMail == true
                ? InboxVerificationState.Valid
                : InboxVerificationState.NotChecked,
            mxCache?.FailureReason,
            mxCache?.AcceptsMail == true,
            mxCache?.Records.OrderBy(x => x.Priority).Select(x => x.Host).ToList() ?? []
        );
        var smtp = new SmtpVerificationEvidence(
            snapshot.State,
            snapshot.FailureReason,
            snapshot.CanConnectSmtp,
            snapshot.HasFullInbox,
            snapshot.IsCatchAll,
            snapshot.IsDeliverable,
            snapshot.IsDisabled
        );
        var evidence = new ProInboxVerificationResult(
            snapshot.State,
            snapshot.FailureReason,
            syntax,
            mx,
            smtp,
            misc
        );
        return new InboxVerificationReport(
            request,
            snapshot.State,
            [evidence],
            snapshot.FailureReason
        );
    }

    private DateTime GetSnapshotExpiration(string domain)
    {
        var lifetime = LongLivedDomains.Contains(domain)
            ? options.Value.LongLivedDomainSnapshotLifetime
            : options.Value.DefaultSnapshotLifetime;
        return DateTime.UtcNow + lifetime;
    }

    private static string? GetDomainSuggestion(string domain)
    {
        return domain switch
        {
            "gmai.com" => "gmail.com",
            "hotmal.com" => "hotmail.com",
            _ => null,
        };
    }

    private sealed record CachedMxResult(
        MxVerificationEvidence Evidence,
        DateTime ExpiresAtUtc,
        long? DatabaseId
    );
}

/// <summary>
/// Pro 验证器组合产生的完整强类型证据。
/// </summary>
public sealed record ProInboxVerificationResult(
    InboxVerificationState State,
    string? FailureReason,
    SyntaxVerificationEvidence Syntax,
    MxVerificationEvidence Mx,
    SmtpVerificationEvidence Smtp,
    MiscVerificationEvidence Misc
) : InboxVerificationEvidence(State, FailureReason);
