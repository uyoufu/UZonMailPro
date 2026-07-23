using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.CorePlugin.Services.Settings;
using UZonMail.DB.Extensions;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.EmailSending;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.ProPlugin.Controllers.Base;
using UZonMail.ProPlugin.Services.Settings.Model;
using UZonMail.ProPlugin.SQL;
using UZonMail.ProPlugin.SQL.ReadingTracker;
using UZonMail.Utils.Web.PagingQuery;
using UZonMail.Utils.Web.ResponseModel;

namespace UZonMail.ProPlugin.Controllers.EmailTracker
{
    public class EmailTrackerController(
        SqlContext db,
        SqlContextPro dbPro,
        TokenService tokenService,
        AppSettingService settingService,
        AppSettingsManager settingsManager
    ) : ControllerBasePro
    {
        private static byte[] _transparentPngBytes =
        [
            0x89,
            0x50,
            0x4E,
            0x47,
            0x0D,
            0x0A,
            0x1A,
            0x0A,
            0x00,
            0x00,
            0x00,
            0x0D,
            0x49,
            0x48,
            0x44,
            0x52,
            0x00,
            0x00,
            0x00,
            0x01,
            0x00,
            0x00,
            0x00,
            0x01,
            0x08,
            0x06,
            0x00,
            0x00,
            0x00,
            0x1F,
            0x15,
            0xC4,
            0x89,
            0x00,
            0x00,
            0x00,
            0x0A,
            0x49,
            0x44,
            0x41,
            0x54,
            0x78,
            0x9C,
            0x63,
            0x00,
            0x01,
            0x00,
            0x00,
            0x05,
            0x00,
            0x01,
            0x0D,
            0x0A,
            0x2D,
            0xB4,
            0x00,
            0x00,
            0x00,
            0x00,
            0x49,
            0x45,
            0x4E,
            0x44,
            0xAE,
            0x42,
            0x60,
            0x82
        ];

        /// <summary>
        /// 读取图片流
        /// </summary>
        /// <param name="trackerId"></param>
        /// <returns></returns>
        [HttpGet("image/{trackerId:length(24)}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStream(string trackerId)
        {
            // 查找文件对象
            var emailAnchor = await dbPro
                .EmailAnchors.Where(x => x.ObjectId == trackerId)
                .Include(x => x.VisitedHistories)
                .FirstOrDefaultAsync();
            if (emailAnchor == null)
                return NotFound();

            // 更新访问次数
            if (emailAnchor.FirstVisitDate > DateTime.UtcNow)
            {
                emailAnchor.FirstVisitDate = DateTime.UtcNow;
            }
            emailAnchor.LastVisitDate = DateTime.UtcNow;
            emailAnchor.VisitedCount += 1;

            // 添加具体的访问历史记录
            // 获取访问的 IP
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(ip))
            {
                var visitHistory = new EmailVisitHistory { IP = ip };
                emailAnchor.VisitedHistories.Add(visitHistory);
            }
            await dbPro.SaveChangesAsync();

            // 更新发送的邮件的状态
            // 判断是否是 cdn 在请求
            if (!IsRequestFromCDN())
            {
                // 更新发送的邮件的状态
                await db.SendingItems.UpdateAsync(
                    x => x.Id == emailAnchor.SendingItemId,
                    x =>
                        x.SetProperty(p => p.Status, SendingItemStatus.Read)
                            .SetProperty(p => p.ReadDate, DateTime.UtcNow)
                );
            }

            // 返回一个像素的透明 png 图片流
            Response.Headers.Append("Content-Disposition", "inline");
            return File(_transparentPngBytes, "image/png");
        }

        /// <summary>
        /// 是否来自 cdn
        /// </summary>
        /// <returns></returns>
        private bool IsRequestFromCDN()
        {
            List<string> cdnHeaders = ["x-forwarded-for", "cf-connecting-ip"];
            var headers = Request.Headers;
            foreach (var header in headers)
            {
                var key = header.Key.ToString();
                if (cdnHeaders.Any(x => key.Contains(x)))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 测试 1x1 的 png 图片
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("test-png")]
        public IActionResult Test1x1Png()
        {
            Response.Headers.Append("Content-Disposition", "inline");
            return File(_transparentPngBytes, "image/png");
        }

        /// <summary>
        /// 获取邮件模板数量
        /// </summary>
        /// <returns></returns>
        [HttpGet("filtered-count")]
        public async Task<ResponseResult<int>> GetSendingGroupsCount(string filter)
        {
            var userId = tokenService.GetUserSqlId();
            var dbSet = dbPro.EmailAnchors.AsNoTracking().Where(x => x.UserId == userId);
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x =>
                    x.InboxEmails.Contains(filter) || x.OutboxEmail.Contains(filter)
                );
            }
            var count = await dbSet.CountAsync();
            return count.ToSuccessResponse();
        }

        /// <summary>
        /// 获取邮件模板数据
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="pagination"></param>
        /// <returns></returns>
        [HttpPost("filtered-data")]
        public async Task<ResponseResult<List<EmailAnchor>>> GetSendingGroupsData(
            string filter,
            Pagination pagination
        )
        {
            var userId = tokenService.GetUserSqlId();
            var dbSet = dbPro.EmailAnchors.AsNoTracking().Where(x => x.UserId == userId);
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x =>
                    x.OutboxEmail.Contains(filter) || x.InboxEmails.Contains(filter)
                );
            }

            var results = await dbSet.Page(pagination).ToListAsync();
            return results.ToSuccessResponse();
        }

        #region 发送设置
        /// <summary>
        /// 获取设置
        /// </summary>
        /// <returns></returns>
        [HttpGet("setting")]
        public async Task<ResponseResult<EmailTrackingSetting>> GetEmailTrackingSetting(
            AppSettingType type = AppSettingType.System
        )
        {
            // 获取设置
            var key = nameof(EmailTrackingSetting);
            var settings = await settingService.GetAppSetting(key, type);
            if (settings == null)
            {
                return new EmailTrackingSetting().ToSuccessResponse();
            }

            return settings.Json!.ToObject<EmailTrackingSetting>()!.ToSuccessResponse();
        }

        /// <summary>
        /// 更新设置
        /// </summary>
        /// <returns></returns>
        [HttpPut("setting")]
        public async Task<ResponseResult<bool>> UpserEmailTrackingSetting(
            [FromBody] EmailTrackingSetting trackingSetting,
            AppSettingType type = AppSettingType.System
        )
        {
            var userId = tokenService.GetUserSqlId();
            // 判断权限
            await settingService.CheckUpdatePermission(userId, type);

            var key = nameof(EmailTrackingSetting);
            var appSetting = await settingService.UpdateAppSetting(trackingSetting, key, type);

            // 更新缓存
            await settingsManager.ResetSetting<EmailTrackingSetting>(appSetting, db);

            return true.ToSuccessResponse();
        }
        #endregion
    }
}
