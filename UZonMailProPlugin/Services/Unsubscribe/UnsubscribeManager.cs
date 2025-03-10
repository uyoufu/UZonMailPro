using Microsoft.EntityFrameworkCore;
using UZonMail.Core.Services.Unsubscribe;
using UZonMail.DB.SQL;
using UZonMail.Utils.Web.Service;
using UZonMailProPlugin.SQL;

namespace UZonMailProPlugin.Services.Unsubscribe
{
    public class UnsubscribeManager(SqlContextPro db) : IUnsubscribeManager, IScopedService<IUnsubscribeManager>
    {
        /// <summary>
        /// 获取组织中所有退订的邮箱
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        public async Task<List<string>> GetUnsubscribedEmails(long organizationId)
        {
            var unsubscribedEmails = await db.UnsubscribeEmails.AsNoTracking()
               .Where(x => x.OrganizationId == organizationId)
               .Where(x => !x.IsDeleted)
               .Select(x => x.Email)
               .ToListAsync();

            return unsubscribedEmails;
        }
    }
}
