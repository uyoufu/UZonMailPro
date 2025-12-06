using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.CorePlugin.Services.Settings;
using UZonMail.DB.Extensions;
using UZonMail.DB.Managers.Cache;
using UZonMail.DB.SQL;
using UZonMail.Utils.Web.PagingQuery;
using UZonMail.Utils.Web.ResponseModel;
using UZonMail.ProPlugin.Controllers.Base;
using UZonMail.ProPlugin.Services.EmailDecorators.JsVariable;
using UZonMail.ProPlugin.SQL;
using UZonMail.ProPlugin.SQL.JsVariable;
using UZonMail.ProPlugin.SQL.ReadingTracker;

namespace UZonMail.ProPlugin.Controllers.JsVariable
{
    /// <summary>
    /// js 变量控制器
    /// </summary>
    public class JsVariableSourceController(SqlContextPro dbPro, TokenService tokenService)
        : ControllerBasePro
    {
        [HttpPost]
        public async Task<ResponseResult<JsVariableSource>> UpsertJsVariableSource(
            [FromBody] JsVariableSource data
        )
        {
            var tokenPayloads = tokenService.GetTokenPayloads();
            data.UserId = tokenPayloads.UserId;
            data.OrganizationId = tokenPayloads.OrganizationId;

            // 判断是否存在
            var existSource = await dbPro
                .JsVariableSources.Where(x =>
                    x.UserId == data.UserId && x.Name == data.Name && x.Id != data.Id
                )
                .FirstOrDefaultAsync();
            if (existSource != null)
                return existSource.ToFailResponse("名称已存在");

            // 若存在 id, 则更新
            if (data.Id > 0)
            {
                await dbPro.JsVariableSources.UpdateAsync(
                    x => x.UserId == data.UserId && x.Id == data.Id,
                    y =>
                        y.SetProperty(m => m.Description, data.Description)
                            .SetProperty(m => m.Name, data.Name)
                            .SetProperty(m => m.Value, data.Value)
                );
                return data.ToSuccessResponse();
            }

            dbPro.JsVariableSources.Add(data);
            await dbPro.SaveChangesAsync();

            // 更新缓存
            DBCacheManager.Global.SetCacheDirty<JsVariableCache>(data.UserId);

            return data.ToSuccessResponse();
        }

        /// <summary>
        /// 获取变量数据数量
        /// </summary>
        /// <returns></returns>
        [HttpGet("filtered-count")]
        public async Task<ResponseResult<int>> GetJsVariableSourceCount(string filter)
        {
            var userId = tokenService.GetUserSqlId();
            var dbSet = dbPro.JsVariableSources.AsNoTracking().Where(x => x.UserId == userId);
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Name.Contains(filter) || x.Description.Contains(filter));
            }
            var count = await dbSet.CountAsync();
            return count.ToSuccessResponse();
        }

        /// <summary>
        /// 获取变量数据数据
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="pagination"></param>
        /// <returns></returns>
        [HttpPost("filtered-data")]
        public async Task<ResponseResult<List<JsVariableSource>>> GetJsVariableSourceData(
            string filter,
            Pagination pagination
        )
        {
            var userId = tokenService.GetUserSqlId();
            var dbSet = dbPro.JsVariableSources.AsNoTracking().Where(x => x.UserId == userId);
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Name.Contains(filter) || x.Description.Contains(filter));
            }

            var results = await dbSet.Page(pagination).ToListAsync();
            return results.ToSuccessResponse();
        }

        /// <summary>
        /// 通过 ids 来删除 js 变量数据
        /// </summary>
        /// <param name="Ids"></param>
        /// <returns></returns>
        [HttpDelete("ids")]
        public async Task<ResponseResult<bool>> DeleteJsVariableSourceDatas(
            [FromBody] List<long> Ids
        )
        {
            var userId = tokenService.GetUserSqlId();
            await dbPro
                .JsVariableSources.Where(x => x.UserId == userId && Ids.Contains(x.Id))
                .ExecuteDeleteAsync();
            return true.ToSuccessResponse();
        }
    }
}
