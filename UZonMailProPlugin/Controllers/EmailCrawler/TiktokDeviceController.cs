using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.CorePlugin.Services.Settings;
using UZonMail.DB.SQL;
using UZonMail.Utils.Web.PagingQuery;
using UZonMail.Utils.Web.ResponseModel;
using UZonMail.ProPlugin.Controllers.Base;
using UZonMail.ProPlugin.Controllers.EmailCrawler.Validators;
using UZonMail.ProPlugin.SQL;
using UZonMail.ProPlugin.SQL.EmailCrawler;

namespace UZonMail.ProPlugin.Controllers.EmailCrawler
{
    public class TiktokDeviceController(SqlContextPro db, TokenService tokenService)
        : ControllerBasePro
    {
        /// <summary>
        /// 创建一个 TikTok 设备
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResponseResult<TikTokDevice>> Create([FromBody] TikTokDevice deviceInfo)
        {
            // 验证数据
            var validator = new TikTokDeviceValidator();
            var validationInfo = validator.Validate(deviceInfo);
            if (!validationInfo.IsValid)
            {
                return new ErrorResponse<TikTokDevice>(validationInfo.Errors[0].ErrorMessage);
            }

            var tokenPayloads = tokenService.GetTokenPayloads();

            // 判断是否重复
            var userId = tokenPayloads.UserId;
            var existOne = await db
                .TikTokDevices.Where(x => x.UserId == userId && x.Name == deviceInfo.Name)
                .FirstOrDefaultAsync();
            if (existOne != null)
            {
                return new ErrorResponse<TikTokDevice>("设备名称已存在");
            }

            // 添加个人信息
            deviceInfo.UserId = userId;
            if (deviceInfo.IsShared)
                deviceInfo.OrganizationId = tokenPayloads.OrganizationId;
            db.TikTokDevices.Add(deviceInfo);
            await db.SaveChangesAsync();

            return deviceInfo.ToSuccessResponse();
        }

        /// <summary>
        /// 删除一个 TikTok 设备
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:long}")]
        public async Task<ResponseResult<bool>> Delete(long id)
        {
            var userId = tokenService.GetUserSqlId();
            var deviceInfo = await db
                .TikTokDevices.Where(x => x.UserId == userId && x.Id == id)
                .FirstOrDefaultAsync();
            if (deviceInfo == null)
            {
                return false.ToFailResponse("设备不存在");
            }

            db.TikTokDevices.Remove(deviceInfo);
            await db.SaveChangesAsync();
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 更新一个 TikTok 设备
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <returns></returns>
        [HttpPut("{id:long}")]
        public async Task<ResponseResult<TikTokDevice>> Update(
            long id,
            [FromBody] TikTokDevice deviceInfo
        )
        {
            // 验证数据
            var validator = new TikTokDeviceValidator();
            var validationInfo = validator.Validate(deviceInfo);
            if (!validationInfo.IsValid)
            {
                return new ErrorResponse<TikTokDevice>(validationInfo.Errors[0].ErrorMessage);
            }

            var tokenPayloads = tokenService.GetTokenPayloads();
            // 判断是否重复
            var userId = tokenPayloads.UserId;

            var oldDeviceInfo = await db
                .TikTokDevices.Where(x => x.UserId == userId && x.Id == id)
                .FirstOrDefaultAsync();
            if (oldDeviceInfo == null)
            {
                return new ErrorResponse<TikTokDevice>("设备不存在");
            }

            var sameNameOne = await db
                .TikTokDevices.AsNoTracking()
                .Where(x => x.UserId == userId && x.Name == deviceInfo.Name && x.Id != id)
                .FirstOrDefaultAsync();
            if (sameNameOne != null)
            {
                return new ErrorResponse<TikTokDevice>("设备名称已存在");
            }

            // 更新信息
            oldDeviceInfo.Name = deviceInfo.Name;
            oldDeviceInfo.Description = deviceInfo.Description;
            oldDeviceInfo.IsShared = deviceInfo.IsShared;
            oldDeviceInfo.OrganizationId = deviceInfo.IsShared ? deviceInfo.OrganizationId : 0;
            oldDeviceInfo.DeviceId = deviceInfo.DeviceId;
            oldDeviceInfo.OdinId = deviceInfo.OdinId;
            await db.SaveChangesAsync();

            return deviceInfo.ToSuccessResponse();
        }

        /// <summary>
        /// 获取数量
        /// </summary>
        /// <returns></returns>
        [HttpGet("filtered-count")]
        public async Task<ResponseResult<int>> GetCount(string filter)
        {
            var tokenPayloads = tokenService.GetTokenPayloads();
            var dbSet = db
                .TikTokDevices.AsNoTracking()
                .Where(x =>
                    x.UserId == tokenPayloads.UserId
                    || x.OrganizationId == tokenPayloads.OrganizationId
                );
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Name.Contains(filter));
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
        public async Task<ResponseResult<List<TikTokDevice>>> GetData(
            string filter,
            Pagination pagination
        )
        {
            var tokenPayloads = tokenService.GetTokenPayloads();
            var dbSet = db
                .TikTokDevices.AsNoTracking()
                .Where(x =>
                    x.UserId == tokenPayloads.UserId
                    || x.OrganizationId == tokenPayloads.OrganizationId
                );
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Name.Contains(filter));
            }

            var results = await dbSet.Page(pagination).ToListAsync();
            return results.ToSuccessResponse();
        }

        /// <summary>
        /// 获取所有的数量
        /// </summary>
        /// <returns></returns>
        [HttpGet("all")]
        public async Task<ResponseResult<List<TikTokDevice>>> GetAllData()
        {
            var tokenPayloads = tokenService.GetTokenPayloads();
            var results = await db
                .TikTokDevices.AsNoTracking()
                .Where(x =>
                    x.UserId == tokenPayloads.UserId
                    || x.OrganizationId == tokenPayloads.OrganizationId
                )
                .ToListAsync();
            return results.ToSuccessResponse();
        }

        /// <summary>
        /// 更新共享状态
        /// </summary>
        /// <returns></returns>
        [HttpPut("{id:long}/is-shared")]
        public async Task<ResponseResult<bool>> UpdateIsShared(long id, bool isShared)
        {
            var tokenPayloads = tokenService.GetTokenPayloads();
            var deviceInfo = await db
                .TikTokDevices.Where(x => x.UserId == tokenPayloads.UserId && x.Id == id)
                .FirstOrDefaultAsync();
            if (deviceInfo == null)
            {
                return false.ToFailResponse("设备不存在");
            }

            deviceInfo.IsShared = isShared;
            deviceInfo.OrganizationId = isShared ? tokenPayloads.OrganizationId : 0;
            await db.SaveChangesAsync();
            return true.ToSuccessResponse();
        }
    }
}
