using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Core.Services.Settings;
using UZonMail.DB.SQL.Core.EmailSending;
using UZonMail.Utils.Web.PagingQuery;
using UZonMail.Utils.Web.ResponseModel;
using UZonMailProPlugin.Controllers.Base;
using UZonMailProPlugin.Services.IpWarmUp;
using UZonMailProPlugin.SQL;
using UZonMailProPlugin.SQL.IPWarmUp;

namespace UZonMailProPlugin.Controllers.IPWarmUp
{
    /// <summary>
    /// IP 预热计划控制器
    /// </summary>
    public class IpWarmUpPlanController(TokenService tokenService, SqlContextPro dbPro, IpWarmUpTaskService ipWarmUpTaskService) : ControllerBasePro
    {
        /// <summary>
        /// 添加 IP 预热计划
        /// 使用 SendStartDate 和 SendEndDate 接收预热周期
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult<IpWarmUpUpPlan>> AddIpWarmUpPlan([FromBody] SendingGroup data)
        {
            var tokenPayloads = tokenService.GetTokenPayloads();
            data.UserId = tokenPayloads.UserId;

            var plan = await ipWarmUpTaskService.CreatePlan(data);
            return plan.ToSuccessResponse();
        }

        /// <summary>
        /// 获取数据数量
        /// </summary>
        /// <returns></returns>
        [HttpGet("filtered-count")]
        public async Task<ResponseResult<int>> GetIpWarmUpPlanCount(string filter)
        {
            var userId = tokenService.GetUserSqlId();
            var dbSet = dbPro.IpWarmUpUpPlans.AsNoTracking().Where(x => x.UserId == userId);
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Name.Contains(filter));
            }
            var count = await dbSet.CountAsync();
            return count.ToSuccessResponse();
        }

        /// <summary>
        /// 获取数据数据
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="pagination"></param>
        /// <returns></returns>
        [HttpPost("filtered-data")]
        public async Task<ResponseResult<List<IpWarmUpUpPlan>>> GetIpWarmUpPlanData(string filter, Pagination pagination)
        {
            var userId = tokenService.GetUserSqlId();
            var dbSet = dbPro.IpWarmUpUpPlans.AsNoTracking().Where(x => x.UserId == userId);
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Name.Contains(filter));
            }

            var results = await dbSet.Page(pagination).ToListAsync();
            return results.ToSuccessResponse();
        }

        /// <summary>
        /// 通过 ids 来删除数据
        /// </summary>
        /// <param name="Ids"></param>
        /// <returns></returns>
        [HttpDelete("ids")]
        public async Task<ResponseResult<bool>> DeleteIpWarmUpPlanDatas([FromBody] List<long> Ids)
        {
            var userId = tokenService.GetUserSqlId();
            await dbPro.IpWarmUpUpPlans.Where(x => x.UserId == userId && Ids.Contains(x.Id)).ExecuteDeleteAsync();
            return true.ToSuccessResponse();
        }
    }
}
