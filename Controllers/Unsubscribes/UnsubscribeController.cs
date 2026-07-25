using log4net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UzonMail.CorePlugin.Services.Settings;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Settings;
using UzonMail.ProPlugin.Controllers.Base;
using UzonMail.ProPlugin.Controllers.Unsubscribes.ResponseModels;
using UzonMail.ProPlugin.Services.Settings.Model;
using UzonMail.ProPlugin.Services.Unsubscribe;
using UzonMail.ProPlugin.SQL;
using UzonMail.ProPlugin.SQL.Unsubscribes;
using UzonMail.Utils.Json;
using UzonMail.Utils.Web.PagingQuery;
using UzonMail.Utils.Web.ResponseModel;
using UzonMail.Utils.Web.Token;

namespace UzonMail.ProPlugin.Controllers.Unsubscribes
{
    /// <summary>
    /// 退订控制器
    /// 退订默认是组织级别的设置
    /// </summary>
    public class UnsubscribeController(
        SqlContext db,
        SqlContextPro dbPro,
        TokenService tokenService,
        IConfiguration configuration,
        UnsubscribeService unsubscribeService,
        AppSettingService settingService
    ) : ControllerBasePro
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UnsubscribeController));

        /// <summary>
        /// 只有管理员可以获取退订设置
        /// </summary>
        /// <returns></returns>
        [HttpGet("setting")]
        public async Task<ResponseResult<UnsubscribeSetting>> GetUnsubscribeSettings(
            AppSettingType type = AppSettingType.System
        )
        {
            // 获取设置
            var key = nameof(UnsubscribeSetting);
            var settings = await settingService.GetAppSetting(key, type);
            if (settings == null)
            {
                return new UnsubscribeSetting().ToSuccessResponse();
            }

            var setting = settings.Json?.ToObject<UnsubscribeSetting>() ?? new UnsubscribeSetting();
            return setting.ToSuccessResponse();
        }

        /// <summary>
        /// 只有管理员可以管理退订设置
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPut("setting")]
        public async Task<ResponseResult<bool>> UpdateUnsubscribeSettings(
            [FromBody] UnsubscribeSetting setting,
            AppSettingType type = AppSettingType.System
        )
        {
            var userId = tokenService.GetUserSqlId();
            // 判断权限
            await settingService.CheckUpdatePermission(userId, type);

            var key = nameof(UnsubscribeSetting);
            await settingService.UpdateAppSetting(setting, key, type);

            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 获取退订状态
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("status")]
        public async Task<ResponseResult<bool>> ChangeUnsubscribeStatus(string token)
        {
            var tokenParams = new TokenParams();
            configuration.GetSection("UnsubscribeTokenParams").Bind(tokenParams);
            if (string.IsNullOrEmpty(tokenParams.Secret))
            {
                _logger.Error("系统未配置取消订阅密钥，用户退定失败");
                return ResponseResult<bool>.Fail("系统未配置取消订阅密钥");
            }

            // 从 token 中解析出 email 和 organizationId
            var tokenPayloads = JWTToken.GetTokenPayloads(token);
            // 获取 email
            var email = tokenPayloads.SelectTokenOrDefault("email", string.Empty);
            var organizationId = tokenPayloads.SelectTokenOrDefault("organizationId", "0");
            if (
                string.IsNullOrEmpty(email)
                || !long.TryParse(organizationId, out var longOrganizationId)
                || longOrganizationId == 0
            )
            {
                var message = "无法解析退订 token";
                _logger.Error(message);
                return ResponseResult<bool>.Fail(message);
            }

            // 添加到退订列表
            var existOne = await dbPro.UnsubscribeEmails.FirstOrDefaultAsync(x =>
                x.Email == email && x.OrganizationId == longOrganizationId
            );
            return (existOne != null).ToSuccessResponse();
        }

        /// <summary>
        /// 解析邮件退订中 token 的 payload
        /// 若退订使用外部链接，可以调用该接口获取 token 中的 email 和 organizationId
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet("token-payloads")]
        public async Task<ResponseResult<UnsubscribePayloads>> GetTokenPayloads(string token)
        {
            // token 是 SendItem 中的 ObjectId
            var sendingItem = await db
                .SendingItems.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ObjectId == token);
            if (sendingItem is null)
                return ResponseResult<UnsubscribePayloads>.Fail("无法解析退订 token");

            var user = await db
                .Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == sendingItem.UserId);
            if (user is null)
                return ResponseResult<UnsubscribePayloads>.Fail("退订邮件所属用户不存在");

            // 从 token 中解析出 email 和 organizationId
            var tokenPayloads = new UnsubscribePayloads(sendingItem)
            {
                OrganizationId = user.OrganizationId
            };
            return tokenPayloads.ToSuccessResponse();
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost()]
        public async Task<ResponseResult<bool>> Unsubscribe(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return ResponseResult<bool>.Fail("token is empty");
            }
            // 获取退订时的 IP 地址
            var host = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await unsubscribeService.Unsubscribe(token, host);
            // 开始
            return result.ToSuccessResponse();
        }

        /// <summary>
        /// 是否已经取消订阅
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("unsubscribed")]
        public async Task<ResponseResult<bool>> IsUnsubscribed(string token)
        {
            var result = await unsubscribeService.IsUnsubscribed(token);
            return result.ToSuccessResponse();
        }

        /// <summary>
        /// 获取退订的数量
        /// </summary>
        /// <returns></returns>
        [HttpGet("filtered-count")]
        public async Task<ResponseResult<int>> GetUnsubscribesCount(string filter)
        {
            var organizationId = tokenService.GetOrganizationId();
            var dbSet = dbPro
                .UnsubscribeEmails.AsNoTracking()
                .Where(x => x.OrganizationId == organizationId);
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x =>
                    x.Email.Contains(filter) || (x.Host ?? string.Empty).Contains(filter)
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
        public async Task<ResponseResult<List<UnsubscribeEmail>>> GetUnsubscribesData(
            string filter,
            Pagination pagination
        )
        {
            var organizationId = tokenService.GetOrganizationId();
            var dbSet = dbPro
                .UnsubscribeEmails.AsNoTracking()
                .Where(x => x.OrganizationId == organizationId);
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x =>
                    x.Email.Contains(filter) || (x.Host ?? string.Empty).Contains(filter)
                );
            }

            var results = await dbSet.Page(pagination).ToListAsync();
            return results.ToSuccessResponse();
        }
    }
}
