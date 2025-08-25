using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Core.Services.Cache;
using UZonMail.Core.Services.Config;
using UZonMail.Core.Services.Settings;
using UZonMail.Utils.Web.Exceptions;
using UZonMail.Utils.Web.PagingQuery;
using UZonMail.Utils.Web.ResponseModel;
using UZonMailProPlugin.Controllers.ApiAccess.Model;
using UZonMailProPlugin.Controllers.Base;
using UZonMailProPlugin.Services.Token;
using UZonMailProPlugin.SQL;
using UZonMailProPlugin.SQL.ApiAccess;

namespace UZonMailProPlugin.Controllers.ApiAccess
{
    /// <summary>
    /// API 访问控制器
    /// </summary>
    public class ApiAccessController(SqlContextPro dbPro, TokenService tokenService, CacheService cacheService,
        ApiAccessService apiAccess, DebugConfig debugConfig) : ControllerBasePro
    {
        [HttpPost]
        public async Task<ResponseResult<AccessToken>> UpsertAccessTokenSource([FromBody] AccessToken data)
        {
            data.UserId = tokenService.GetUserSqlId();

            // 验证数据
            if (string.IsNullOrEmpty(data.Name))
            {
                return ResponseResult<AccessToken>.Fail("令牌名称不能为空");
            }

            if (data.ExpireDate <= DateTime.UtcNow)
            {
                return ResponseResult<AccessToken>.Fail("令牌过期时间必须大于当前时间");
            }

            // 判断是否有 redis 服务
            if (!cacheService.IsRedis && !debugConfig.IsDemo)
            {
                throw new KnownException("当前功能需要启用 Redis 缓存");
            }


            // 若存在 id, 则更新
            if (data.Id > 0)
            {
                // 获取现有数据
                var existOne = dbPro.AccessTokens.Where(x => x.Id == data.Id && x.UserId == data.UserId).FirstOrDefault();
                if (existOne == null)
                {
                    return ResponseResult<AccessToken>.Fail("令牌不存在");
                }

                existOne.Name = data.Name;
                existOne.Description = data.Description;
                existOne.Enable = data.Enable;

                // 将令牌 Id 保存到黑名单中
                var blacklistKey = existOne.GetBlacklistKey();
                if (existOne.Enable)
                {
                    // 移除黑名单中的令牌 Id
                    await cacheService.KeyDeleteAsync(blacklistKey);
                }
                else
                {
                    var expireTimeSpan = existOne.ExpireDate - DateTime.UtcNow;
                    if (expireTimeSpan.TotalSeconds > 10)
                        // 添加到黑名单中
                        await cacheService.SetAsync(blacklistKey, existOne.Id, expireTimeSpan);
                }
                await dbPro.SaveChangesAsync();
                return data.ToSuccessResponse();
            }

            // 不存在时，添加
            data.InitJwtId();
            // 生成 apiToken
            var apiToken = await apiAccess.GenerateApiAccessToken(data.UserId, data.JwtId, data.ExpireDate);

            dbPro.AccessTokens.Add(data);
            await dbPro.SaveChangesAsync();

            AccessToken resultData = new AccessTokenResult(data, apiToken);
            return resultData.ToSuccessResponse();
        }

        /// <summary>
        /// 获取数量
        /// </summary>
        /// <returns></returns>
        [HttpGet("filtered-count")]
        public async Task<ResponseResult<int>> GetAccessTokenSourceCount(string filter)
        {
            var userId = tokenService.GetUserSqlId();
            var dbSet = dbPro.AccessTokens.AsNoTracking().Where(x => x.UserId == userId);
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
        public async Task<ResponseResult<List<AccessToken>>> GetAccessTokenSourceData(string filter, Pagination pagination)
        {
            var userId = tokenService.GetUserSqlId();
            var dbSet = dbPro.AccessTokens.AsNoTracking().Where(x => x.UserId == userId);
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Name.Contains(filter) || x.Description.Contains(filter));
            }

            var results = await dbSet.Page(pagination).ToListAsync();
            return results.ToSuccessResponse();
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="Ids"></param>
        /// <returns></returns>
        [HttpDelete("{id:length(24)}")]
        public async Task<ResponseResult<bool>> DeleteAccessTokenSourceDatas(string id)
        {
            var userId = tokenService.GetUserSqlId();
            var existOne = await dbPro.AccessTokens.Where(x => x.ObjectId == id && x.UserId == userId).FirstOrDefaultAsync();
            if (existOne == null)
            {
                return true.ToSuccessResponse();
            }

            // 添加到黑名单
            var expireTimeSpan = existOne.ExpireDate - DateTime.UtcNow;
            if (expireTimeSpan.TotalSeconds > 10)
                // 添加到黑名单中
                await cacheService.SetAsync(existOne.GetBlacklistKey(), existOne.Id, expireTimeSpan);

            // 删除
            dbPro.AccessTokens.Remove(existOne);
            await dbPro.SaveChangesAsync();

            return true.ToSuccessResponse();
        }
    }
}
