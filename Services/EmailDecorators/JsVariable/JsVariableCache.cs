using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using UzonMail.DB.Managers.Cache;
using UzonMail.ProPlugin.SQL;
using UzonMail.ProPlugin.SQL.JsVariable;

namespace UzonMail.ProPlugin.Services.EmailDecorators.JsVariable;

/// <summary>
/// JS 变量函数及数据源的原始快照。
/// </summary>
public sealed record JsVariableSnapshot(
    IReadOnlyDictionary<string, JsFunctionDefinition> Functions,
    JObject Source
);

/// <summary>
/// 按用户聚合的 JS 变量派生缓存。
/// </summary>
public sealed class JsVariableCache : BaseDBCache<SqlContextPro, long>
{
    public long UserId => Args;

    public IReadOnlyDictionary<string, JsFunctionDefinition> Functions { get; private set; } =
        new Dictionary<string, JsFunctionDefinition>();

    public JObject Source { get; private set; } = [];

    /// <summary>
    /// 获取指定用户的 JS 变量原始源键。
    /// </summary>
    public static CacheSourceKey<JsVariableSnapshot, long> GetSourceKey(long userId) => new(userId);

    /// <inheritdoc />
    protected override async Task UpdateCore(
        CacheBuildContext buildContext,
        SqlContextPro db,
        CancellationToken cancellationToken
    )
    {
        var snapshot = await buildContext.GetSourceAsync(
            GetSourceKey(UserId),
            async token =>
            {
                var functions = await db
                    .JsFunctionDefinitions.AsNoTracking()
                    .Where(x => x.UserId == UserId)
                    .ToDictionaryAsync(x => x.Name, token);
                var sources = await db
                    .JsVariableSources.AsNoTracking()
                    .Where(x => x.UserId == UserId)
                    .ToListAsync(token);
                var sourceValues = new JObject();
                foreach (var source in sources)
                    sourceValues.Add(source.Name, source.Value);

                return new JsVariableSnapshot(functions, sourceValues);
            },
            cancellationToken
        );

        Functions = snapshot.Functions;
        Source = (JObject)snapshot.Source.DeepClone();
    }
}
