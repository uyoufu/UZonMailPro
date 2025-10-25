using UZonMail.DB.SQL.Core.EmailSending;

namespace UZonMailProPlugin.Controllers.IPWarmUp.DTOs
{
    public class WarmUpPlanData : SendingGroup
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 发送数量图表数据点
        /// </summary>
        public List<double[]> SendCountChartPoints { get; set; } = [ ];
    }
}
