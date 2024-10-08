﻿using log4net;
using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Unsubscribes;
using UZonMail.Utils.Web.Exceptions;
using UZonMail.Utils.Web.ResponseModel;
using UZonMail.Utils.Web.Service;

namespace UZonMailProPlugin.Services.Unsubscribe
{
    public class UnsubscribeService(SqlContext db) : IScopedService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UnsubscribeService));
        /// <summary>
        /// 开始退订
        /// </summary>
        /// <param name="sendingItemId"></param>
        /// <returns></returns>
        public async Task<bool> Unsubscribe(string sendingItemId)
        {
            var sendingItem = await db.SendingItems.AsNoTracking().FirstOrDefaultAsync(x => x.ObjectId == sendingItemId);
            if (sendingItem == null)
            {
                var message = "无法解析退订 token";
                _logger.Error(message);
                throw new KnownException(message);
            }
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == sendingItem.UserId);

            var toEmails = sendingItem.ToEmails.Split(',');
            foreach(var toEmail in toEmails)
            {
                // 添加到退订列表
                var existOne = await db.UnsubscribeEmails.FirstOrDefaultAsync(x => x.OrganizationId == user.OrganizationId && x.Email == toEmail);
                if (existOne != null)
                {
                    continue;
                }

                // 新增退订
                var unsubscribeEmail = new UnsubscribeEmail()
                {
                    Email = toEmail,
                    OrganizationId = user.OrganizationId,
                };
                db.Add(unsubscribeEmail);
            }            
            await db.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 是否已经退订
        /// </summary>
        /// <param name="sendingItemId"></param>
        /// <returns></returns>
        public async Task<bool> IsUnsubscribed(string sendingItemId)
        {
            var sendingItem = await db.SendingItems.AsNoTracking().FirstOrDefaultAsync(x => x.ObjectId == sendingItemId);
            if (sendingItem == null)
            {
                var message = "无法解析退订 token";
                _logger.Error(message);
                throw new KnownException(message);
            }
            var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == sendingItem.UserId);
            var toEmails = sendingItem.ToEmails.Split(',');
            var existCount = await db.UnsubscribeEmails.CountAsync(x => x.OrganizationId == user.OrganizationId && toEmails.Contains(x.Email));
            return existCount == toEmails.Length;
        }
    }
}