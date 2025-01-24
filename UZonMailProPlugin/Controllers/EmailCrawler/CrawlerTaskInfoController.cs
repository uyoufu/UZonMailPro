using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Core.Services.Settings;
using UZonMail.Core.Utils.Database;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.EmailCrawler;
using UZonMail.Utils.Json;
using UZonMail.Utils.Web.Exceptions;
using UZonMail.Utils.Web.PagingQuery;
using UZonMail.Utils.Web.ResponseModel;
using UZonMailProPlugin.Controllers.Base;
using UZonMailProPlugin.Services.Crawlers;
using UZonMailProPlugin.Services.Crawlers.ByteDance.APIs;
using UZonMailProPlugin.Services.Crawlers.ByteDance.Extensions;

namespace UZonMailProPlugin.Controllers.EmailCrawler
{
    /// <summary>
    /// 邮件爬虫控制器
    /// </summary>
    public class CrawlerTaskInfoController(SqlContext db, TokenService tokenService, CrawlerManager crawlerManager, IHttpClientFactory httpClientFactory) : ControllerBasePro
    {
        /// <summary>
        /// 获取所有的爬虫类型
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResponseResult<List<CrawlerType>>> GetCrawlerTypes()
        {
            return Enum.GetValues(typeof(CrawlerType))
                .Cast<CrawlerType>()
                .ToList()
                .ToSuccessResponse();
        }

        /// <summary>
        /// 创建一个爬虫
        /// </summary>
        /// <param name="crawlerTaskInfo"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult<CrawlerTaskInfo>> CreateCrawler([FromBody] CrawlerTaskInfo crawlerTaskInfo)
        {
            // 验证数据
            if (string.IsNullOrEmpty(crawlerTaskInfo.Name))
            {
                throw new KnownException("爬虫名称不能为空");
            }

            // 添加个人信息
            var userId = tokenService.GetUserSqlId();
            crawlerTaskInfo.UserId = userId;

            await db.CrawlerTaskInfos.AddAsync(crawlerTaskInfo);
            await db.SaveChangesAsync();

            return crawlerTaskInfo.ToSuccessResponse();
        }

        /// <summary>
        /// 更新爬虫信息
        /// 只更新 Name, Description, UserProxyId
        /// </summary>
        /// <param name="crawlerTaskInfo"></param>
        /// <returns></returns>
        [HttpPut("{crawlerTaskId:long}")]
        public async Task<ResponseResult<bool>> UpdateCrawler(long crawlerTaskId, [FromBody] CrawlerTaskInfo crawlerTaskInfo)
        {
            var userId = tokenService.GetUserSqlId();
            await db.CrawlerTaskInfos.UpdateAsync(x => x.Id == crawlerTaskId && x.UserId == userId,
                x => x.SetProperty(y => y.Name, crawlerTaskInfo.Name)
                    .SetProperty(y => y.Description, crawlerTaskInfo.Description)
                    .SetProperty(y => y.ProxyId, crawlerTaskInfo.ProxyId)
                    .SetProperty(y=>y.TikTokDeviceId,crawlerTaskInfo.TikTokDeviceId)
                    .SetProperty(y => y.Deadline, crawlerTaskInfo.Deadline)
                );
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 删除爬虫
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{crawlerTaskId:long}")]
        public async Task<ResponseResult<bool>> DeleteCrawler(long crawlerTaskId)
        {
            var userId = tokenService.GetUserSqlId();
            var deletedCount = await db.CrawlerTaskInfos.UpdateAsync(x => x.Id == crawlerTaskId && x.UserId == userId, x => x.SetProperty(y => y.IsDeleted, true));

            // 移除爬虫任务
            if (deletedCount > 0) await crawlerManager.StopCrawler(crawlerTaskId, db);


            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 启动爬虫
        /// </summary>
        /// <returns></returns>
        [HttpPut("{crawlerTaskId:long}/start")]
        public async Task<ResponseResult<bool>> StartCrawler(long crawlerTaskId)
        {
            var userId = tokenService.GetUserSqlId();
            var crawlerTaskInfo = await db.CrawlerTaskInfos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == crawlerTaskId && x.UserId == userId);
            if (crawlerTaskInfo == null)
            {
                return false.ToFailResponse("未找到爬虫任务");
            }

            // 根据类型启动不同的爬虫
            if (crawlerTaskInfo.Type == CrawlerType.TikTokEmail)
            {
                // 启动TikTok爬虫
                crawlerManager.StartTikTokEmailCrawler(crawlerTaskInfo);
            }
            else
            {
                return false.ToFailResponse("暂不支持的爬虫类型");
            }

            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 停止爬虫
        /// </summary>
        /// <returns></returns>
        [HttpPut("{crawlerTaskId:long}/stop")]
        public async Task<ResponseResult<bool>> StopCrawler(long crawlerTaskId)
        {
            var userId = tokenService.GetUserSqlId();
            var crawlerTaskInfo = await db.CrawlerTaskInfos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == crawlerTaskId && x.UserId == userId);
            if (crawlerTaskInfo == null)
            {
                return false.ToFailResponse("未找到爬虫任务");
            }
            await crawlerManager.StopCrawler(crawlerTaskId, db);
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 获取数量
        /// </summary>
        /// <returns></returns>
        [HttpGet("filtered-count")]
        public async Task<ResponseResult<int>> GetSendingGroupsCount(string filter)
        {
            var userId = tokenService.GetUserSqlId();
            var dbSet = db.CrawlerTaskInfos.AsNoTracking().Where(x => x.UserId == userId);
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Name.Contains(filter) || x.Description.Contains(filter));
            }
            var count = await dbSet.CountAsync();
            return count.ToSuccessResponse();
        }

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="pagination"></param>
        /// <returns></returns>
        [HttpPost("filtered-data")]
        public async Task<ResponseResult<List<CrawlerTaskInfo>>> GetSendingGroupsData(string filter, Pagination pagination)
        {
            var userId = tokenService.GetUserSqlId();
            var dbSet = db.CrawlerTaskInfos.AsNoTracking().Where(x => x.UserId == userId);
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Name.Contains(filter) || x.Description.Contains(filter));
            }

            var results = await dbSet.Page(pagination).ToListAsync();
            return results.ToSuccessResponse();
        }

        /// <summary>
        /// 测试接口
        /// </summary>
        /// <returns></returns>
        [HttpGet("test")]
        [AllowAnonymous]
        public async Task<ResponseResult<List<string>>> Test()
        {
            var httpClient = httpClientFactory.CreateClient("tiktok");
            httpClient.AddUserAgentHeaders();
            var result = await new GetRecommendList()
                .WithHttpClient(httpClient)
                .GetJsonAsync();

            var itemList = result.SelectTokenOrDefault<JArray>("itemList");
            if(itemList==null)return new List<string>().ToSuccessResponse();

            var authorIds = itemList.Select(x => x.SelectTokenOrDefault<string>("author.id")).ToList();
            // 请求网络
            return authorIds.ToSuccessResponse();
        }
    }
}
