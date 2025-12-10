using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.CorePlugin.Services.SendCore;
using UZonMail.CorePlugin.Services.Settings;
using UZonMail.DB.Extensions;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.EmailSending;
using UZonMail.ProPlugin.Controllers.Base;
using UZonMail.ProPlugin.Controllers.IPWarmUp.DTOs;
using UZonMail.ProPlugin.Services.IpWarmUp;
using UZonMail.ProPlugin.SQL;
using UZonMail.ProPlugin.SQL.IPWarmUp;
using UZonMail.Utils.Web.PagingQuery;
using UZonMail.Utils.Web.ResponseModel;

namespace UZonMail.ProPlugin.Controllers.IPWarmUp
{
    /// <summary>
    /// IP 预热计划控制器
    /// </summary>
    public class IpWarmUpPlanController(
        TokenService tokenService,
        SqlContextPro dbPro,
        SqlContext db,
        IpWarmUpTaskService ipWarmUpTaskService,
        SendingGroupService sendingGroupService
    ) : ControllerBasePro
    {
        /// <summary>
        /// 添加 IP 预热计划
        /// 使用 SendStartDate 和 SendEndDate 接收预热周期
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult<IpWarmUpUpPlan>> AddIpWarmUpPlan(
            [FromBody] WarmUpPlanData data
        )
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
        public async Task<ResponseResult<List<IpWarmUpUpPlan>>> GetIpWarmUpPlanData(
            string filter,
            Pagination pagination
        )
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

            // 查找属于自己的计划
            var selfPlans = await dbPro
                .IpWarmUpUpPlans.Where(x => x.UserId == userId && Ids.Contains(x.Id))
                .ToListAsync();

            // 停止对应的发件任务
            var sendingGroupIds = await dbPro
                .IpWarmUpUpTasks.Where(x => selfPlans.Select(x => x.Id).Contains(x.IPWarmUpPlanId))
                .Select(x => x.SendingGroupId)
                .ToListAsync();
            var sendingGroups = await db
                .SendingGroups.AsNoTracking()
                .Where(x => sendingGroupIds.Contains(x.Id))
                .ToListAsync();
            foreach (var sendingGroup in sendingGroups)
            {
                await sendingGroupService.RemoveSendingGroupTask(sendingGroup);

                await sendingGroupService.UpdateSendingGroupStatus(
                    sendingGroup.Id,
                    SendingGroupStatus.Pause,
                    "手动取消"
                );
            }

            // 删除计划
            await dbPro
                .IpWarmUpUpPlans.Where(x => x.UserId == userId && Ids.Contains(x.Id))
                .ExecuteDeleteAsync();

            // 删除关联的定时任务
            foreach (var planId in Ids)
            {
                await ipWarmUpTaskService.DeletePlanSchedule(planId);
            }

            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 获取指定预热计划的最新发送组 ID
        /// </summary>
        /// <param name="planObjectId"></param>
        /// <returns></returns>
        [HttpGet("sendingGroupIds/latest")]
        public async Task<ResponseResult<long>> GetLatestSendingGroupOfSchedulePlan(
            string planObjectId
        )
        {
            var userId = tokenService.GetUserSqlId();

            var plan = await dbPro
                .IpWarmUpUpPlans.AsNoTracking()
                .Where(x => x.UserId == userId && x.ObjectId == planObjectId)
                .FirstOrDefaultAsync();
            if (plan == null)
                return 0L.ToSuccessResponse();

            var planTask = await dbPro
                .IpWarmUpUpTasks.Where(x => x.IPWarmUpPlanId == plan.Id)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();
            if (planTask == null)
                return 0L.ToSuccessResponse();

            return planTask.SendingGroupId.ToSuccessResponse();
        }
    }
}
