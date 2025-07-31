using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Uamazing.Utils.Web.ResponseModel;
using UZonMail.Core.Services.SendCore;
using UZonMail.Core.Services.Settings;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.EmailSending;
using UZonMail.Utils.Web.ResponseModel;
using UZonMailProPlugin.Controllers.Api.Model;
using UZonMailProPlugin.Controllers.Base;
using UZonMailProPlugin.Services.License;
using UZonMailProPlugin.Utils;

namespace UZonMailProPlugin.Controllers.Api
{
    /// <summary>
    /// 邮件发送 Api
    /// </summary>
    public class EmailSendingController(SqlContext db,
        LicenseAccessService licenseAccessService,
        SendingGroupService sendingService, TokenService tokenService
        ) : ControllerBaseBusiness
    {
        /// <summary>
        /// 立即发件
        /// </summary>
        /// <returns></returns>
        [HttpPost()]
        [Authorize(Roles = ApiRoles.ApiUser)]
        public async Task<ResponseResult<SendingGroup>> CreateSendingTask([FromBody] SendingGroupData sendingData)
        {
            // 验证授权是否过期
            if (!await licenseAccessService.HasEnterpriseLicense())
            {
                return ResponseResult<SendingGroup>.Fail("您的授权已过期，请联系管理员续费。", HttpStatusCode.Unauthorized);
            }

            // 用户名
            var userId = tokenService.GetUserSqlId();
            // 将数据转换为 sendingGroup 对象
            var sendingGroup = await sendingData.ConvertToSendingGroup(db);            
            sendingGroup.UserId = userId;


            return await sendingService.StartSending(sendingGroup);
        }

        [HttpGet("groups/{objectId:length(24)}")]
        public async Task<ResponseResult<SendingGroup>> GetSendingGroupInfo(string objectId)
        {
            var sendingGroup = await db.SendingGroups.Where(x=>x.ObjectId == objectId)
                 .Select(x => new SendingGroup()
                 {
                     Id = x.Id,
                     ObjectId = x.ObjectId,
                     Subjects = x.Subjects,
                     SendingType = x.SendingType,
                     Status = x.Status,
                     Templates = x.Templates,
                     Outboxes = x.Outboxes, // 兼容旧数据
                     OutboxesCount = x.OutboxesCount,
                     InboxesCount = x.InboxesCount,
                     SuccessCount = x.SuccessCount,
                     SentCount = x.SentCount,
                     CreateDate = x.CreateDate,
                     TotalCount = x.TotalCount,
                     ScheduleDate = x.ScheduleDate,
                 })
                .FirstOrDefaultAsync();
            if(sendingGroup==null)return ResponseResult<SendingGroup>.Fail("未找到对应的发送任务。");

            // 返回结果
            return sendingGroup.ToSuccessResponse();
        }
    }
}
