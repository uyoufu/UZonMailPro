using UZonMail.DB.SQL.Base;
using UZonMail.DB.SQL.Core.EmailSending;

namespace UZonMail.ProPlugin.SQL.IPWarmUp
{
    /// <summary>
    /// IP 预热具体的任务
    /// </summary>
    public class IpWarmUpUpTask : SqlId
    {
        /// <summary>
        /// 计划 id
        /// </summary>
        public long IPWarmUpPlanId { get; set; }
        public IpWarmUpUpPlan IPWarmUpPlan { get; set; }

        /// <summary>
        /// 实际发送组的 Id
        /// </summary>
        public long SendingGroupId { get; set; }

        /// <summary>
        /// 实际开始时间
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 实际结束时间
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// 当前任务的发件箱数量
        /// </summary>
        public int OutboxesCount { get; set; }

        /// <summary>
        /// 收件箱数量
        /// </summary>
        public int InboxesCount { get; set; }

        /// <summary>
        /// 成功数据
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// 成功或者失败的消息体
        /// </summary>
        public string Message { get; set; }
    }
}
