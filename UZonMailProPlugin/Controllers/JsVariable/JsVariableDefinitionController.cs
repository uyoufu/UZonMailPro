using Jint;
using Jint.Native;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Core.Services.Settings;
using UZonMail.DB.Extensions;
using UZonMail.DB.Managers.Cache;
using UZonMail.Utils.Web.PagingQuery;
using UZonMail.Utils.Web.ResponseModel;
using UZonMailProPlugin.Controllers.Base;
using UZonMailProPlugin.Services.EmailDecorators.JsVariable;
using UZonMailProPlugin.SQL;
using UZonMailProPlugin.SQL.JsVariable;

namespace UZonMailProPlugin.Controllers.JsVariable
{
    /// <summary>
    /// 变量定义控制器
    /// </summary>
    /// <param name="dbPro"></param>
    /// <param name="tokenService"></param>
    public class JsFunctionDefinitionController(SqlContextPro dbPro, TokenService tokenService) : ControllerBasePro
    {
        [HttpPost]
        public async Task<ResponseResult<JsFunctionDefinition>> UpsertJsFunctionDefinition([FromBody] JsFunctionDefinition data)
        {
            var tokenPayloads = tokenService.GetTokenPayloads();
            data.UserId = tokenPayloads.UserId;
            data.OrganizationId = tokenPayloads.OrganizationId;

            // 判断是否存在
            var existSource = await dbPro.JsFunctionDefinitions
                .Where(x => x.UserId == data.UserId && x.Name == data.Name && x.Id != data.Id)
                .FirstOrDefaultAsync();
            if (existSource != null)
                return existSource.ToFailResponse("名称已存在");

            // 若存在 id, 则更新
            if (data.Id > 0)
            {
                await dbPro.JsFunctionDefinitions.UpdateAsync(x => x.UserId == data.UserId && x.Id == data.Id,
                    y => y.SetProperty(m => m.Description, data.Description)
                    .SetProperty(m => m.Name, data.Name)
                    .SetProperty(m => m.FunctionBody, data.FunctionBody)
                );
                return data.ToSuccessResponse();
            }

            dbPro.JsFunctionDefinitions.Add(data);
            await dbPro.SaveChangesAsync();

            // 更新缓存
            CacheManager.Global.SetCacheDirty<JsVariableCache>(data.UserId);

            return data.ToSuccessResponse();
        }

        /// <summary>
        /// 获取变量数据数量
        /// </summary>
        /// <returns></returns>
        [HttpGet("filtered-count")]
        public async Task<ResponseResult<int>> GetJsFunctionDefinitionCount(string filter)
        {
            var userId = tokenService.GetUserSqlId();
            var dbSet = dbPro.JsFunctionDefinitions.AsNoTracking().Where(x => x.UserId == userId);
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
        public async Task<ResponseResult<List<JsFunctionDefinition>>> GetJsFunctionDefinitionData(string filter, Pagination pagination)
        {
            var userId = tokenService.GetUserSqlId();
            var dbSet = dbPro.JsFunctionDefinitions.AsNoTracking().Where(x => x.UserId == userId);
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
        public async Task<ResponseResult<bool>> DeleteJsFunctionDefinitionDatas([FromBody] List<long> Ids)
        {
            var userId = tokenService.GetUserSqlId();
            await dbPro.JsFunctionDefinitions.Where(x => x.UserId == userId && Ids.Contains(x.Id)).ExecuteDeleteAsync();
            return true.ToSuccessResponse();
        }

        [HttpGet("test/{definitionId:long}")]
        public async Task<ResponseResult<string>> TestJsFunctionDefinition(long definitionId)
        {
            var userId = tokenService.GetUserSqlId();
            var definition = await dbPro.JsFunctionDefinitions.Where(x => x.UserId == userId && definitionId == x.Id).FirstOrDefaultAsync();
            if (definition == null)
                return ResponseResult<string>.Fail("未找到对应的函数定义");

            var jsVariableCache = await CacheManager.Global.GetCache<JsVariableCache, SqlContextPro>(dbPro, userId);
            var uzonData = UzonData.GetTestUzonData(jsVariableCache);

            // 开始测试
            try
            {
                var jintEngin = new Engine();
                var jsValue = jintEngin.SetValue("uzonData", uzonData).Execute(definition.GetFunctionDefinition()).Invoke(definition.Name);
                var serializer = new Jint.Native.Json.JsonSerializer(jintEngin);
                string result = serializer.Serialize(jsValue).ToString();
                return result.ToSuccessResponse();
            }
            catch (Exception ex)
            {
                return ResponseResult<string>.Fail($"函数执行错误: {ex.Message}");
            }
        }
    }
}
