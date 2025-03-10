using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Core.Services.Settings;
using UZonMail.Core.Utils.Database;
using UZonMail.DB.SQL;
using UZonMail.DB.Utils;
using UZonMail.Utils.Web.PagingQuery;
using UZonMail.Utils.Web.ResponseModel;
using UZonMailProPlugin.Controllers.Base;
using UZonMailProPlugin.SQL;
using UZonMailProPlugin.SQL.Unsubscribes;

namespace UZonMailProPlugin.Controllers.Unsubscribes
{
    /// <summary>
    /// 退订页面控制器
    /// </summary>
    /// <param name="dbPro"></param>
    /// <param name="tokenService"></param>
    public class UnsubscribePageController(SqlContext db, SqlContextPro dbPro, TokenService tokenService) : ControllerBasePro
    {
        /// <summary>
        /// 添加退订页面
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost()]
        public async Task<ResponseResult<UnsubscribePage>> CreateUnsubscribePage([FromBody] UnsubscribePage unsubscribePage)
        {
            if (string.IsNullOrEmpty(unsubscribePage.Language)) return ResponseResult<UnsubscribePage>.Fail("languageRequired");
            if (string.IsNullOrEmpty(unsubscribePage.HtmlContent)) return ResponseResult<UnsubscribePage>.Fail("htmlContentRequired");

            // 添加当前用户的相关信息
            var organizationId = tokenService.GetOrganizationId();

            var unsubscribe = await dbPro.UnsubscribePages.FirstOrDefaultAsync(x =>
                x.OrganizationId == organizationId && x.Language == unsubscribePage.Language);
            if (unsubscribe != null)
            {
                return ResponseResult<UnsubscribePage>.Fail("unsubscribePageHasAreadyExist");
            }

            unsubscribePage.OrganizationId = organizationId;
            dbPro.Add(unsubscribePage);
            await dbPro.SaveChangesAsync();

            return unsubscribePage.ToSuccessResponse();
        }

        /// <summary>
        /// 更新退订页面内容
        /// </summary>
        /// <param name="id">退订页的id</param>
        /// <param name="htmlContent"></param>
        /// <returns></returns>
        [HttpPut("{id:long}/content")]
        public async Task<ResponseResult<bool>> UpdateUnsubscribePage(long id, [FromBody] UnsubscribePage unsubscribePage)
        {
            var organizationId = tokenService.GetOrganizationId();
            var htmlContent = unsubscribePage.HtmlContent;

            await dbPro.UnsubscribePages.UpdateAsync(x => x.OrganizationId == organizationId && x.Id == id, x => x.SetProperty(y => y.HtmlContent, htmlContent));
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 删除退订页面
        /// 只能删除属于本人的退订页面
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:long}")]
        public async Task<ResponseResult<bool>> DeleteUnsubscribePage(long id)
        {
            var organizationId = tokenService.GetOrganizationId();
            var exist = await dbPro.UnsubscribePages.FirstOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == id);
            if (exist == null) return true.ToSuccessResponse();

            // 开始删除
            dbPro.UnsubscribePages.Remove(exist);
            await dbPro.SaveChangesAsync();
            return true.ToSuccessResponse();
        }

        /// <summary>
        /// 获取退订页面
        /// 先通过 token 获取
        /// 若没有 token, 则通过 id 获取
        /// 最后根据语言获取
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet()]
        public async Task<ResponseResult<UnsubscribePage>> GetUnsubscribePage(string token, long id, string language)
        {
            long organizationId = 0;
            if (!string.IsNullOrEmpty(token))
            {
                // 获取邮件对应的组织
                var sendingItem = await db.SendingItems.FirstOrDefaultAsync(x => x.ObjectId == token);
                if (sendingItem == null) return ResponseResult<UnsubscribePage>.Fail("Token invalid");
                var user = await db.Users.FirstOrDefaultAsync(x => x.Id == sendingItem.UserId);
                organizationId = user.OrganizationId;
            }
            else
            {
                organizationId = tokenService.GetOrganizationId();
            }
            
            UnsubscribePage? pageResult = null;
            if (id > 0)
            {
                pageResult = await dbPro.UnsubscribePages.Where(x => x.Id == id)
                    .Where(x => x.OrganizationId == organizationId || x.IsDefault)
                    .FirstOrDefaultAsync();
            }
            if (pageResult == null && !string.IsNullOrEmpty(language))
            {
                // 从语言中获取
                pageResult = await dbPro.UnsubscribePages.Where(x => x.Language == language)
                    .Where(x => x.OrganizationId == organizationId || x.IsDefault)
                    .FirstOrDefaultAsync();
            }
            // 如果未找到，则从默认的退订页面中获取
            pageResult ??= await dbPro.UnsubscribePages.Where(x => x.IsDefault || x.OrganizationId == organizationId).FirstOrDefaultAsync();
            pageResult ??= new UnsubscribePage()
            {
                OrganizationId = organizationId,
                HtmlContent = "Unsubscribe page is missing",
                Language = "en-US"
            };

            return pageResult.ToSuccessResponse();
        }

        /// <summary>
        /// 获取邮件模板数量
        /// </summary>
        /// <returns></returns>
        [HttpGet("filtered-count")]
        public async Task<ResponseResult<int>> GetUnsubscribePagesCount(string filter)
        {
            var organizationId = tokenService.GetOrganizationId();
            var dbSet = dbPro.UnsubscribePages.AsNoTracking().Where(x => x.OrganizationId == organizationId || x.IsDeleted);
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Language.Contains(filter) || x.HtmlContent.Contains(filter));
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
        public async Task<ResponseResult<List<UnsubscribePage>>> GetUnsubscribePagesData(string filter, Pagination pagination)
        {
            var organizationId = tokenService.GetOrganizationId();
            var dbSet = dbPro.UnsubscribePages.AsNoTracking().Where(x => x.OrganizationId == organizationId || x.IsDeleted);
            if (!string.IsNullOrEmpty(filter))
            {
                dbSet = dbSet.Where(x => x.Language.Contains(filter) || x.HtmlContent.Contains(filter));
            }

            var results = await dbSet.Page(pagination).ToListAsync();
            return results.ToSuccessResponse();
        }
    }
}
