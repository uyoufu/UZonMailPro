using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using UZonMail.Core.Utils.Database;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.EmailCrawler;

namespace UZonMailProPlugin.Services.Crawlers.TiTok
{
    public abstract class CrawlStep
    {
        private readonly HashSet<long> _crawlerTaskIds = [];
        private static readonly object _lock = new();

        protected CrawlerTaskParams CrawlerTaskParams { get; }
        /// <summary>
        /// 表示作者的 Id
        /// </summary>
        public string Key { get; }

        public CrawlStep(long authorId)
        {
            Key = authorId.ToString();
            StartAsync();
        }

        public CrawlStep()
        {
            Key = Guid.NewGuid().ToString();
            StartAsync();
        }

        private Task _executeTask;
        /// <summary>
        /// 执行的任务
        /// </summary>
        public virtual Task ExecuteTask => _executeTask;

        /// <summary>
        /// 添加爬虫任务 ID
        /// </summary>
        /// <param name="taskId"></param>
        public void AddCrawlerTaskId(long taskId)
        {
            if (taskId <= 0) return;

            lock (_lock)
            {
                _crawlerTaskIds.Add(taskId);
            }
        }

        public Task StartAsync()
        {
            _executeTask ??= ExecuteAsync();
            return _executeTask;
        }

        public void StopAsync(long taskId)
        {
            _crawlerTaskIds.Remove(taskId);
            foreach (var item in _children)
            {
                item.Value.StopAsync(taskId);
            }
        }

        protected abstract Task ExecuteAsync();

        private ConcurrentDictionary<string, CrawlStep> _parents { get; } = [];
        private ConcurrentDictionary<string, CrawlStep> _children { get; } = [];

        /// <summary>
        /// 设置父级
        /// 同时将当前任务添加到父级的子任务中
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public CrawlStep AddParent(CrawlStep parent)
        {
            if (parent == null) return this;

            _parents.TryAdd(parent.Key, parent);
            parent._children.TryAdd(Key, this);
            return this;
        }

        /// <summary>
        /// 移除父级
        /// </summary>
        /// <param name="parentKey"></param>
        public void RemoveParent(string parentKey)
        {
            if (_parents.TryRemove(parentKey, out var parent))
            {
                parent._children.TryRemove(Key, out _);
            }
        }

        public void RemoveParent(CrawlStep parent)
        {
            RemoveParent(parent.Key);
        }

        /// <summary>
        /// 获取当前和所有父级的任务 ID
        /// </summary>
        /// <returns></returns>
        private List<long> GetAllTaskIds()
        {
            var hashSet = new HashSet<long>();
            foreach (var item in _crawlerTaskIds)
            {
                hashSet.Add(item);
            }

            // 判断是否有父级
            // 获取所有的父级任务 ID
            foreach (var item in _parents)
            {
                var parentSet = item.Value.GetAllTaskIds();
                parentSet.ForEach(x => hashSet.Add(x));
            }

            return [.. hashSet];
        }

        public bool ExistsCrawlerTaskId(long taskId)
        {
            if (_crawlerTaskIds.Contains(taskId)) return true;

            foreach (var item in _parents)
            {
                if (item.Value.ExistsCrawlerTaskId(taskId)) return true;
            }
            return false;
        }

        /// <summary>
        /// 是否应该停止
        /// </summary>
        /// <returns></returns>
        public bool ShouldStop()
        {
            if (_crawlerTaskIds.Count > 0) return false;
            // 判断父级是否全部退出
            var someNotEmpty = _parents.Any(x => !x.Value.ShouldStop());
            return !someNotEmpty;
        }

        /// <summary>
        /// 保存爬虫任务结果
        /// </summary>
        /// <param name="db"></param>
        /// <param name="authorId"></param>
        /// <returns></returns>
        public async Task SaveCrawlerTaskResult(SqlContext db, TiktokAuthor tiktokAuthor)
        {
            var taskIds = GetAllTaskIds();
            var exists = await db.CrawlerTaskResults.AsNoTracking()
                .Where(x => x.TikTokAuthorId == tiktokAuthor.Id)
                .Where(x => taskIds.Contains(x.CrawlerTaskInfoId))
                .Select(x => x.CrawlerTaskInfoId)
                .Distinct()
                .ToListAsync();

            // 移除已经存在的任务
            foreach (var item in exists)
            {
                taskIds.Remove(item);
            }

            // 保存剩余的任务
            foreach (var taskId in taskIds)
            {
                await db.CrawlerTaskResults.AddAsync(new CrawlerTaskResult
                {
                    CrawlerTaskInfoId = taskId,
                    TikTokAuthorId = tiktokAuthor.Id,
                    ExistExtraInfo = tiktokAuthor.IsParsed
                });

                // 增加数量
                await db.CrawlerTaskInfos.UpdateAsync(x => x.Id == taskId, x => x.SetProperty(y => y.Count, y => y.Count + 1));
            }
            await db.SaveChangesAsync();
        }
    }
}
