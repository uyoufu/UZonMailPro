using Innofactor.EfCoreJsonValueConverter;
using Newtonsoft.Json.Linq;
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
        public string Subjects { get; set; } = string.Empty;

        /// <summary>
        /// 模板列表
        /// </summary>
        [JsonField]
        public List<long> TemplateIds { get; set; } = [];

        /// <summary>
        /// 发件箱列表
        /// 数据中的发件箱不应添加到此处，系统会自动从数据中提取
        /// </summary>
        [JsonField]
        public List<long> OutboxIds { get; set; } = [];

        /// <summary>
        /// 收件箱列表
        /// </summary>
        [JsonField]
        public List<long> InboxIds { get; set; } = [];

        /// <summary>
        /// 抄送 ids
        /// </summary>
        [JsonField]
        public List<long> CcIds { get; set; } = [];

        /// <summary>
        /// 密送 ids
        /// </summary>
        [JsonField]
        public List<long> BccIds { get; set; } = [];

        /// <summary>
        /// 附件 ids
        /// </summary>
        [JsonField]
        public List<long> AttachmentIds { get; set; } = [];

        /// <summary>
        /// 用户数据
        /// </summary>
        [JsonField]
        public JArray? Data { get; set; }

        /// <summary>
        /// 正文内容
        /// </summary>
        public string Body { get; set; }
         
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// 成功的任务数
        /// </summary>
        public int SuccessTasksCount { get; set; }

        /// <summary>
        /// 当前收件箱的索引
        /// </summary>
        public long CurrentInboxIndex { get; set; } = 0;

        /// <summary>
        /// 状态
        /// </summary>
        public IpWarmUpUpStatus Status { get; set; }
    }
}
