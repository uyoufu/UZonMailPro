using Microsoft.EntityFrameworkCore;
using UzonMail.DB.SQL.Base;

namespace UzonMail.ProPlugin.SQL.EmailVerify;

/// <summary>
/// 按域名缓存的 MX 查询结果。
/// </summary>
[Index(nameof(Domain), IsUnique = true)]
public sealed class MxDomainCache : SqlId
{
    public string Domain { get; set; } = string.Empty;

    public bool AcceptsMail { get; set; }

    public string? FailureReason { get; set; }

    public DateTime CheckedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }

    public List<MxDomainRecord> Records { get; set; } = [];
}

/// <summary>
/// 一个 MX 主机记录。
/// </summary>
public sealed class MxDomainRecord : SqlId
{
    public long MxDomainCacheId { get; set; }

    public MxDomainCache MxDomainCache { get; set; } = null!;

    public string Host { get; set; } = string.Empty;

    public int Priority { get; set; }
}
