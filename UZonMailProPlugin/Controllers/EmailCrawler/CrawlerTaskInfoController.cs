using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Core.Services.Settings;
using UZonMail.Core.Utils.Database;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.EmailCrawler;
using UZonMail.DB.SQL.Emails;
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
                    .SetProperty(y => y.TikTokDeviceId, crawlerTaskInfo.TikTokDeviceId)
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
        public async Task<ResponseResult<int>> GetCrawlerTasksCount(string filter)
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
        public async Task<ResponseResult<List<CrawlerTaskInfo>>> GetCrawlerTasksData(string filter, Pagination pagination)
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
        /// 获取数量信息
        /// </summary>
        /// <param name="crawlerTaskIds"></param>
        /// <returns></returns>
        [HttpPost("count-infos")]
        public async Task<ResponseResult<List<CrawlerTaskInfo>>> GetCrawlerTasksCount([FromBody] List<long> crawlerTaskIds)
        {
            if (crawlerTaskIds.Count == 0) return new List<CrawlerTaskInfo>().ToSuccessResponse();

            // 获取数量
            var results = await db.CrawlerTaskInfos.AsNoTracking()
                .Where(x => crawlerTaskIds.Contains(x.Id))
                .Select(x => new CrawlerTaskInfo()
                {
                    Id = x.Id,
                    Count = x.Count
                })
                .ToListAsync();
            return results.ToSuccessResponse();
        }

        /// <summary>
        /// 获取数量
        /// </summary>
        /// <returns></returns>
        [HttpGet("{crawlerTaskId:long}/results/filtered-count")]
        public async Task<ResponseResult<int>> GetCrawlerTaskResultsCount(long crawlerTaskId, string filter)
        {
            var dbSet = db.CrawlerTaskResults.AsNoTracking()
                .Where(x => x.CrawlerTaskInfoId == crawlerTaskId)
                .Where(x => x.ExistExtraInfo)
                .Include(x => x.TiktokAuthor)
                .Select(x => x.TiktokAuthor)
                .Where(x => !string.IsNullOrEmpty(x.Email));

            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Email.Contains(filter));
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
        [HttpPost("{crawlerTaskId:long}/results/filtered-data")]
        public async Task<ResponseResult<List<TiktokAuthor>>> GetCrawlerTaskResultsData(long crawlerTaskId, string filter, Pagination pagination)
        {
            var userId = tokenService.GetUserSqlId();
            // 判断是否属于用户可访问
            var crawlerTask = db.CrawlerTaskInfos.AsNoTracking()
                .Where(x => x.UserId == userId && x.Id == crawlerTaskId)
                .Select(x => x.Id)
                .FirstAsync();
            if (crawlerTask == null) return ResponseResult<List<TiktokAuthor>>.Fail("未找到爬虫任务");

            var dbSet = db.CrawlerTaskResults.AsNoTracking()
                .Where(x => x.CrawlerTaskInfoId == crawlerTaskId)
                .Where(x => x.ExistExtraInfo)
                .Include(x => x.TiktokAuthor)
                .Select(x => x.TiktokAuthor)
                .Where(x => !string.IsNullOrEmpty(x.Email))
                .Select(x => new TiktokAuthor()
                {
                    Id = x.Id,
                    Email = x.Email,
                    CreateDate = x.CreateDate,
                    Nickname = x.Nickname,
                });

            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Email.Contains(filter));
            }

            var results = await dbSet.Page(pagination).ToListAsync();
            return results.ToSuccessResponse();
        }

        /// <summary>
        /// 保存爬虫结果到收件箱
        /// </summary>
        /// <param name="crawlerTaskId"></param>
        /// <returns></returns>
        [HttpPost("{crawlerTaskId:long}/inbox-group")]
        public async Task<ResponseResult<long>> SaveCrawlerResultsAaInbox(long crawlerTaskId)
        {
            var tokenPayloads = tokenService.GetTokenPayloads();

            var crawlerTask = await db.CrawlerTaskInfos
                .Where(x => x.Id == crawlerTaskId)
                .Where(x => x.UserId == tokenPayloads.UserId)
                .FirstAsync();

            if (crawlerTask == null)
            {
                return ResponseResult<long>.Fail("未找到爬虫任务");
            }

            // 开始另存
            // 创建发件箱组
            if (crawlerTask.OutboxGroupId == 0)
            {
                // 新建发件箱组
                var outboxGroup = new EmailGroup()
                {
                    UserId = crawlerTask.UserId,
                    Name = $"爬虫结果:{crawlerTask.Name}",
                    Description = $"来源于爬虫任务 {crawlerTask.Name}",
                    Type = EmailGroupType.InBox,
                };
                await db.EmailGroups.AddAsync(outboxGroup);
                await db.SaveChangesAsync();
                crawlerTask.OutboxGroupId = outboxGroup.Id;
            }

            // 保存收件箱
            var tiktokAuthors = await db.CrawlerTaskResults.AsNoTracking()
                .Where(x => x.CrawlerTaskInfoId == crawlerTaskId)
                .Where(x => x.ExistExtraInfo)
                .Where(x => !x.IsAttachingInbox)
                .Include(x => x.TiktokAuthor)
                .Select(x => new TiktokAuthor()
                {
                    Id = x.TikTokAuthorId,
                    Email = x.TiktokAuthor.Email,
                    Nickname = x.TiktokAuthor.Nickname,
                })
                .Where(x => !string.IsNullOrEmpty(x.Email))
                .ToListAsync();
            if (tiktokAuthors.Count == 0)
            {
                return crawlerTask.OutboxGroupId.ToSuccessResponse();
            }

            var inboxes = tiktokAuthors.Select(x => new Inbox()
            {
                EmailGroupId = crawlerTask.OutboxGroupId,
                Name = x.Nickname,
                Email = x.Email,
                Description = $"来源于爬虫任务 {crawlerTask.Name}",
                UserId = tokenPayloads.UserId,
                OrganizationId = tokenPayloads.OrganizationId
            })
                .DistinctBy(x => x.Email)
                .ToList();

            // 若不存在，则添加
            foreach (var inbox in inboxes)
            {
                var existOne = await db.Inboxes.AsNoTracking().Where(x => x.Email == inbox.Email && x.UserId == inbox.UserId)
                    .Select(x => new { x.Id })
                    .FirstOrDefaultAsync();
                if (existOne != null) continue;
                await db.Inboxes.AddAsync(inbox);
            }
            // 标记已经转换成 outbox
            await db.CrawlerTaskResults.UpdateAsync(x => x.CrawlerTaskInfoId == crawlerTaskId, x => x.SetProperty(y => y.IsAttachingInbox, true));
            await db.SaveChangesAsync();

            return crawlerTask.OutboxGroupId.ToSuccessResponse();
        }
    }
}
