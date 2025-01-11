using Microsoft.Extensions.DependencyInjection;
using UZonMail.Core.Utils.Database;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.EmailCrawler;
using UZonMail.Utils.Web.Service;

namespace UZonMailProPlugin.Services.Crawlers
{
    /// <summary>
    /// 爬虫任务基类
    /// 参考 BackgroundService
    /// </summary>
    public abstract class ClawerTaskService(SqlContext sqlContext, CrawlerManager crawlerManager) : IScopedService
    {
        private Task? _executeTask;
        private CancellationTokenSource? _stoppingCts;
        private IServiceScope? _scope;
        private long _crawlerTaskId;

        private async Task DisposeScope()
        {
            // 从容器中移除
            crawlerManager.TryRemove(_crawlerTaskId, out _);

            // 更新数据库状态
            await sqlContext.CrawlerTaskInfos.UpdateAsync(x => x.Id == _crawlerTaskId,
                x => x.SetProperty(y => y.Status, CrawlerStatus.Stopped)
                    .SetProperty(y => y.EndDate, DateTime.Now)
            );

            _scope?.Dispose();
        }

        /// <summary>
        /// Gets the Task that executes the background operation.
        /// </summary>
        /// <remarks>
        /// Will return <see langword="null"/> if the background operation hasn't started.
        /// </remarks>
        public virtual Task? ExecuteTask => _executeTask;

        /// <summary>
        /// This method is called when the <see cref="IHostedService"/> starts. The implementation should return a task that represents
        /// the lifetime of the long running operation(s) being performed.
        /// </summary>
        /// <param name="stoppingToken">Triggered when <see cref="IHostedService.StopAsync(CancellationToken)"/> is called.</param>
        /// <returns>A <see cref="Task"/> that represents the long running operations.</returns>
        /// <remarks>See <see href="https://docs.microsoft.com/dotnet/core/extensions/workers">Worker Services in .NET</see> for implementation guidelines.</remarks>
        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous Start operation.</returns>
        public virtual async Task StartAsync(IServiceScope scope, long crawlerTaskId)
        {
            _scope = scope;
            _crawlerTaskId = crawlerTaskId;
            // 保存到单例服务中
            crawlerManager.TryAdd(_crawlerTaskId, this);
            // 更新数据库状态
            await sqlContext.CrawlerTaskInfos.UpdateAsync(x => x.Id == _crawlerTaskId,
                x => x.SetProperty(y => y.Status, CrawlerStatus.Running)
                    .SetProperty(y => y.StartDate, DateTime.Now)
            );

            _stoppingCts = new CancellationTokenSource();
            // Store the task we're executing
            _executeTask = ExecuteAsync(_stoppingCts.Token);
            await _executeTask;

            await DisposeScope();
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous Stop operation.</returns>
        public virtual async Task StopAsync()
        {
            // Stop called without start
            if (_executeTask == null)
            {
                return;
            }

            try
            {
                // Signal cancellation to the executing method
                _stoppingCts!.Cancel();
            }
            finally
            {
                await _executeTask!.ConfigureAwait(false);
            }

            await DisposeScope();
        }

        /// <inheritdoc />
        public virtual void Dispose()
        {
            _stoppingCts?.Cancel();
        }
    }
}
