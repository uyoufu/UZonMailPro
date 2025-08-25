using log4net;
using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL;
using UZonMail.Utils.Http;
using UZonMail.Utils.Web.Service;
using UZonMailProPlugin.Services.Crawlers.ByteDance.Extensions;
using UZonMailProPlugin.Services.Crawlers.TikTok;
using UZonMailProPlugin.Services.License;
using UZonMailProPlugin.SQL;
using UZonMailProPlugin.SQL.EmailCrawler;

namespace UZonMailProPlugin.Services.Crawlers
{
    /// <summary>
    /// 爬虫任务基类
    /// 参考 BackgroundService
    /// </summary>
    public abstract class CrawlerTaskBase(IServiceProvider serviceProvider) : IScopedService
    {
        private readonly static ILog _logger = LogManager.GetLogger(typeof(CrawlerTaskBase));
        protected readonly AsyncServiceScope Scope = serviceProvider.CreateAsyncScope();

        private CrawlerTaskParams _crawlerTaskParams;
        protected RootStep RootStep { get; private set; }

        /// <summary>
        /// 子类重写此方法，执行具体的爬取任务
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected abstract Task ExecuteAsync(CrawlerTaskParams crawlerTaskParams);

        /// <summary>
        /// 外部调用，开始执行
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="crawlerTaskId"></param>
        /// <returns></returns>
        public async Task StartAsync(IServiceScope scope, long crawlerTaskId)
        {
            // Store the task we're executing
            // 判断是否有权限调用
            var access = scope.ServiceProvider.GetRequiredService<LicenseAccessService>();
            var hasAccess = await access.HasTiktokEmailCrawlerAccess();
            if (!hasAccess)
            {
                return;
            }

            var dbPro = scope.ServiceProvider.GetRequiredService<SqlContextPro>();
            var crawlerTaskInfo = await dbPro.CrawlerTaskInfos.AsNoTracking().Where(x => x.Id == crawlerTaskId).FirstOrDefaultAsync();

            if (crawlerTaskInfo?.TikTokDeviceId == 0)
            {
                _logger.Warn("设备信息为空");
                return;
            }

            // 查找参数
            var device = await dbPro.TikTokDevices.AsNoTracking().Where(x => x.Id == crawlerTaskInfo.TikTokDeviceId).FirstOrDefaultAsync();
            if (device == null)
            {
                _logger.Warn($"设备信息不存在: {crawlerTaskInfo.TikTokDeviceId}");
                return;
            }

            var httpClientHandler = new HttpClientHandler();
            if (crawlerTaskInfo?.ProxyId > 0)
            {
                var db = scope.ServiceProvider.GetRequiredService<SqlContext>();
                // 开始使用代理
                var proxyStr = await GetProxyString(db, crawlerTaskInfo.ProxyId);
                if (!string.IsNullOrEmpty(proxyStr))
                {
                    httpClientHandler.WithProxy(proxyStr);
                }
            };
            var httpClient = new HttpClient(httpClientHandler);
            httpClient.AddUserAgentHeaders();

            _crawlerTaskParams = new CrawlerTaskParams()
            {
                ServiceProvider = Scope.ServiceProvider,
                StepManager = Scope.ServiceProvider.GetRequiredService<CrawlStepManager>(),
                DeviceId = device.DeviceId,
                CrawlerTaskId = crawlerTaskId,
                OdinId = device.OdinId,
                HttpClient = httpClient,
            };
            RootStep = new RootStep(crawlerTaskId);

            // 标记任务开始
            await dbPro.CrawlerTaskInfos.Where(x => x.Id == crawlerTaskId).ExecuteUpdateAsync(x => x.SetProperty(y => y.Status, CrawlerStatus.Running)
                .SetProperty(y => y.StartDate, DateTime.UtcNow));

            try
            {
                // Execute the task
                await ExecuteAsync(_crawlerTaskParams);
            }
            catch (Exception ex)
            {
                _logger.Error($"爬虫任务{crawlerTaskId}异常终止", ex);
            }
            finally
            {
                // 任务结束
                // 计算结果数量
                var resultCount = await dbPro.CrawlerTaskResults
                    .Where(x => x.CrawlerTaskInfoId == crawlerTaskId)
                    .CountAsync();

                // 标记任务结束
                await dbPro.CrawlerTaskInfos.Where(x => x.Id == crawlerTaskId && x.Status == CrawlerStatus.Running)
                    .ExecuteUpdateAsync(x => x.SetProperty(y => y.Status, CrawlerStatus.Stopped)
                        .SetProperty(y => y.EndDate, DateTime.UtcNow)
                        .SetProperty(y => y.Count, resultCount));

                httpClient.Dispose();
            }
        }

        public async Task<bool> RestartAsync(long crawlerTaskId)
        {
            if (RootStep == null) return false;

            RootStep.AddCrawlerTaskId(crawlerTaskId);
            _crawlerTaskParams.CrawlerTaskId = crawlerTaskId;
            return true;
        }

        private async Task<string?> GetProxyString(SqlContext db, long proxyId)
        {
            var proxy = await db.Proxies.Where(x => x.Id == proxyId).FirstOrDefaultAsync();
            return proxy?.Url;
        }

        /// <summary>
        /// 停止
        /// 由外部调用，可能是在其它线程中操作
        /// </summary>
        /// <returns></returns>
        public virtual async Task StopAsync(long crawlerTaskId)
        {
            // 将所有涉及到的资源释放
            RootStep?.StopAsync(crawlerTaskId);
        }
    }
}
