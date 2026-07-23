using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using UZonMail.DB.Managers.Cache;
using UZonMail.ProPlugin.SQL;
using UZonMail.ProPlugin.SQL.JsVariable;

namespace UZonMail.ProPlugin.Services.EmailDecorators.JsVariable
{
    public class JsVariableCache : BaseDBCache<SqlContextPro, long>
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public long UserId => Args;

        public override void Dispose() { }

        protected override async Task UpdateCore(SqlContextPro db)
        {
            Functions.Clear();
            Source.RemoveAll();

            // 获取函数
            var functions = await db
                .JsFunctionDefinitions.AsNoTracking()
                .Where(x => x.UserId == UserId)
                .ToListAsync();
            foreach (var function in functions)
            {
                Functions.TryAdd(function.Name, function);
            }

            // 获取数据源
            var sources = await db
                .JsVariableSources.AsNoTracking()
                .Where(x => x.UserId == UserId)
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
