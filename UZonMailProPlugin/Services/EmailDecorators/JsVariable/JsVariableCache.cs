using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using UZonMail.DB.Managers.Cache;
using UZonMail.DB.SQL;
using UZonMailProPlugin.SQL;
using UZonMailProPlugin.SQL.JsVariable;

namespace UZonMailProPlugin.Services.EmailDecorators.JsVariable
{
    public class JsVariableCache : BaseDBCache<SqlContextPro>
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public long UserId => LongValue;

        public override void Dispose()
        {
        }

        public override async Task Update(SqlContextPro db)
        {
            if (!NeedUpdate) return;
            Functions.Clear();
            Source.RemoveAll();

            // 获取函数
            var functions = await db.JsFunctionDefinitions.AsNoTracking().Where(x => x.UserId == UserId)
                .ToListAsync();
            foreach (var function in functions)
            {
                Functions.TryAdd(function.Name, function);
            }

            // 获取数据源
            var sources = await db.JsVariableSources.AsNoTracking().Where(x => x.UserId == UserId)
                .ToListAsync();
            foreach (var source in sources)
            {
                Source.Add(source.Name, source.Value);
            }
        }

        /// <summary>
        /// 函数
        /// </summary>
        public ConcurrentDictionary<string, JsFunctionDefinition> Functions { get; } = [];

        /// <summary>
        /// 数据源
        /// </summary>
        public JObject Source { get; } = [];

    }
}
