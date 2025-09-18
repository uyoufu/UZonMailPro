using Innofactor.EfCoreJsonValueConverter;
using UZonMail.DB.SQL.Base;

namespace UZonMailProPlugin.SQL.IPWarmUp
{
    public class IpWarmUpUpPlan : SqlId
    {
        public long UserId { get; set; }

        /// <summary>
        /// 计划名称，主要用于区分不同的计划
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 主题
        /// </summary>
        [JsonField]
        public List<string> Subjects { get; set; } = [];

        /// <summary>
        /// 模板列表
        /// </summary>
        [JsonField]
        public List<long> TemplateIds { get; set; } = [];

        /// <summary>
        /// 发件箱列表
        /// </summary>
        [JsonField]
        public List<long> OutboxIds { get; set; } = [];

        /// <summary>
        /// 收件箱列表
        /// </summary>
        [JsonField]
        public List<long> InboxIds { get; set; } = [];

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// 计划任务数量
        /// </summary>
        public int ScheduleTasksCount { get; set; }

        /// <summary>
        /// 当前任务索引
        /// </summary>
        public int CurrentTaskIndex { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public IpWarmUpUpStatus Status { get; set; }
    }
}
