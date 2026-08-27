using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UzonMail.CorePlugin.Services.Config;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.CorePlugin.Utils.Database;
using UzonMail.DB.Extensions;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Emails;
using UzonMail.ProPlugin.Controllers.Base;
using UzonMail.ProPlugin.Services.Crawlers;
using UzonMail.ProPlugin.SQL;
using UzonMail.ProPlugin.SQL.EmailCrawler;
using UzonMail.Utils.Web.Exceptions;
using UzonMail.Utils.Web.PagingQuery;
using UzonMail.Utils.Web.ResponseModel;

namespace UzonMail.ProPlugin.Controllers.EmailCrawler
{
    /// <summary>
    /// 邮件爬虫控制器
    /// </summary>
    public class CrawlerTaskInfoController(
        SqlContext db,
        SqlContextPro dbPro,
        TokenService tokenService,
        CrawlerManager crawlerManager,
        DebugConfig debugConfig
    ) : ControllerBasePro
    {
        /// <summary>
        /// 获取所有的爬虫类型
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ResponseResult<List<CrawlerType>>> GetCrawlerTypes()
        {
            return Enum.GetValues<CrawlerType>().Cast<CrawlerType>().ToList().ToSuccessResponse();
        }

        /// <summary>
        /// 创建一个爬虫
        /// </summary>
        /// <param name="crawlerTaskInfo"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult<CrawlerTaskInfo>> CreateCrawler(
            [FromBody] CrawlerTaskInfo crawlerTaskInfo
        )
        {
            // 验证数据
            if (string.IsNullOrEmpty(crawlerTaskInfo.Name))
            {
                throw new KnownException("爬虫名称不能为空");
            }

            // 添加个人信息
            var userId = tokenService.GetUserSqlId();
            crawlerTaskInfo.UserId = userId;

            await dbPro.CrawlerTaskInfos.AddAsync(crawlerTaskInfo);
            await dbPro.SaveChangesAsync();

            return crawlerTaskInfo.ToSuccessResponse();
        }

        /// <summary>
        /// 更新爬虫信息
        /// 只更新 Name, Description, UserProxyId
        /// </summary>
        /// <param name="crawlerTaskInfo"></param>
        /// <returns></returns>
        [HttpPut("{crawlerTaskId:long}")]
        public async Task<ResponseResult<bool>> UpdateCrawler(
            long crawlerTaskId,
            [FromBody] CrawlerTaskInfo crawlerTaskInfo
        )
        {
            var userId = tokenService.GetUserSqlId();
            await dbPro.CrawlerTaskInfos.UpdateAsync(
                x => x.Id == crawlerTaskId && x.UserId == userId,
                x =>
                    x.SetProperty(y => y.Name, crawlerTaskInfo.Name)
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
            var deletedCount = await dbPro.CrawlerTaskInfos.UpdateAsync(
                x => x.Id == crawlerTaskId && x.UserId == userId,
                x => x.SetProperty(y => y.IsDeleted, true)
            );

            // 移除爬虫任务
            if (deletedCount > 0)
                await crawlerManager.StopCrawler(crawlerTaskId);

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
            var crawlerTaskInfo = await dbPro
                .CrawlerTaskInfos.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == crawlerTaskId && x.UserId == userId);
            if (crawlerTaskInfo == null)
            {
                return false.ToFailResponse("未找到爬虫任务");
            }

            if (debugConfig.IsDemo)
            {
                return false.ToFailResponse("当前为示例状态，爬虫无法执行");
            }

            // 根据类型启动不同的爬虫
            if (crawlerTaskInfo.Type == CrawlerType.TikTokEmail)
            {
                // 启动TikTok爬虫
                crawlerManager.StartTikTokEmailCrawlerInBackground(crawlerTaskInfo);
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
            var crawlerTaskInfo = await dbPro
                .CrawlerTaskInfos.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == crawlerTaskId && x.UserId == userId);
            if (crawlerTaskInfo == null)
            {
                return false.ToFailResponse("未找到爬虫任务");
            }
            await crawlerManager.StopCrawler(crawlerTaskId);
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
            var dbSet = dbPro.CrawlerTaskInfos.AsNoTracking().Where(x => x.UserId == userId);
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x =>
                    x.Name.Contains(filter) || (x.Description ?? string.Empty).Contains(filter)
                );
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
        public async Task<ResponseResult<List<CrawlerTaskInfo>>> GetCrawlerTasksData(
            string filter,
            Pagination pagination
        )
        {
            var userId = tokenService.GetUserSqlId();
            var dbSet = dbPro.CrawlerTaskInfos.AsNoTracking().Where(x => x.UserId == userId);
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x =>
                    x.Name.Contains(filter) || (x.Description ?? string.Empty).Contains(filter)
                );
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
        public async Task<ResponseResult<List<CrawlerTaskInfo>>> GetCrawlerTasksCount(
            [FromBody] List<long> crawlerTaskIds
        )
        {
            if (crawlerTaskIds.Count == 0)
                return new List<CrawlerTaskInfo>().ToSuccessResponse();

            // 获取数量
            var results = await dbPro
                .CrawlerTaskInfos.AsNoTracking()
                .Where(x => crawlerTaskIds.Contains(x.Id))
                .Select(x => new CrawlerTaskInfo() { Id = x.Id, Count = x.Count })
                .ToListAsync();
            return results.ToSuccessResponse();
        }

        /// <summary>
        /// 获取数量
        /// </summary>
        /// <returns></returns>
        [HttpGet("{crawlerTaskId:long}/results/filtered-count")]
        public async Task<ResponseResult<int>> GetCrawlerTaskResultsCount(
            long crawlerTaskId,
            string filter
        )
        {
            var dbSet = dbPro
                .CrawlerTaskResults.AsNoTracking()
                .Where(x => x.CrawlerTaskInfoId == crawlerTaskId)
                .Where(x => x.ExistExtraInfo)
                .Include(x => x.TiktokAuthor)
                .Select(x => x.TiktokAuthor)
                .Where(x => !string.IsNullOrEmpty(x.Email));

            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => (x.Email ?? string.Empty).Contains(filter));
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
        public async Task<ResponseResult<List<TiktokAuthor>>> GetCrawlerTaskResultsData(
            long crawlerTaskId,
            string filter,
            Pagination pagination
        )
        {
            var userId = tokenService.GetUserSqlId();
            // 判断是否属于用户可访问
            var crawlerTaskExists = await dbPro
                .CrawlerTaskInfos.AsNoTracking()
                .AnyAsync(x => x.UserId == userId && x.Id == crawlerTaskId);
            if (!crawlerTaskExists)
                return ResponseResult<List<TiktokAuthor>>.Fail("未找到爬虫任务");

            var dbSet = dbPro
                .CrawlerTaskResults.AsNoTracking()
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
                dbSet = dbSet.Where(x => (x.Email ?? string.Empty).Contains(filter));
            }

            var results = await dbSet.Page(pagination).ToListAsync();
            return results.ToSuccessResponse();
        }

        /// <summary>
        /// 保存爬虫结果到收件联系人组
        /// </summary>
        /// <param name="crawlerTaskId"></param>
        /// <returns></returns>
        [HttpPost("{crawlerTaskId:long}/recipient-contact-group")]
        public async Task<ResponseResult<long>> SaveCrawlerResultsAsRecipientContacts(
            long crawlerTaskId
        )
        {
            var tokenPayloads = tokenService.GetTokenPayloads();

            var crawlerTask = await dbPro
                .CrawlerTaskInfos.Where(x => x.Id == crawlerTaskId)
                .Where(x => x.UserId == tokenPayloads.UserId)
                .FirstAsync();

            if (crawlerTask == null)
            {
                return ResponseResult<long>.Fail("未找到爬虫任务");
            }

            // 开始另存
            // 每个爬虫任务复用同一个结果联系人组。
            if (crawlerTask.RecipientContactGroupId == 0)
            {
                var recipientContactGroup = new EmailGroup()
                {
                    UserId = crawlerTask.UserId,
                    Name = $"爬虫结果:{crawlerTask.Name}",
                    Description = $"来源于爬虫任务 {crawlerTask.Name}",
                    Category = EmailGroupCategory.RecipientEmail,
                };
                await db.EmailGroups.AddAsync(recipientContactGroup);
                await db.SaveChangesAsync();

                crawlerTask.RecipientContactGroupId = recipientContactGroup.Id;
            }

            // 保存尚未归档的收件联系人。
            var tiktokAuthors = await dbPro
                .CrawlerTaskResults.AsNoTracking()
                .Where(x => x.CrawlerTaskInfoId == crawlerTaskId)
                .Where(x => x.ExistExtraInfo)
                .Where(x => !x.IsAttachingRecipientContact)
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
                return crawlerTask.RecipientContactGroupId.ToSuccessResponse();
            }

            var recipientContacts = tiktokAuthors
                .Select(x => new RecipientContact()
                {
                    EmailGroupId = crawlerTask.RecipientContactGroupId,
                    Name = x.Nickname,
                    Email = x.Email ?? string.Empty,
                    Description = $"来源于爬虫任务 {crawlerTask.Name}",
                    UserId = tokenPayloads.UserId,
                    OrganizationId = tokenPayloads.OrganizationId
                })
                .DistinctBy(x => x.Email)
                .ToList();

            // 若不存在，则添加
            foreach (var recipientContact in recipientContacts)
            {
                var existOne = await db
                    .RecipientContacts.AsNoTracking()
                    .Where(x =>
                        x.Email == recipientContact.Email && x.UserId == recipientContact.UserId
                    )
                    .Select(x => new { x.Id })
                    .FirstOrDefaultAsync();
                if (existOne != null)
                    continue;
                await db.RecipientContacts.AddAsync(recipientContact);
            }
            // 标记已经转换成收件联系人，避免重复导入。
            await dbPro.CrawlerTaskResults.UpdateAsync(
                x => x.CrawlerTaskInfoId == crawlerTaskId,
                x => x.SetProperty(y => y.IsAttachingRecipientContact, true)
            );
            await dbPro.SaveChangesAsync();
            await db.SaveChangesAsync();

            return crawlerTask.RecipientContactGroupId.ToSuccessResponse();
        }
    }
}
