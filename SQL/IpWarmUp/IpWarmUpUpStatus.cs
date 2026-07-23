namespace UZonMail.ProPlugin.SQL.IPWarmUp
{
    public enum IpWarmUpUpStatus
    {
        /// <summary>
        /// 计划创建完成，等待开始
        /// </summary>
        Created = 0,
        /// <summary>
        /// 计划正在进行中
        /// </summary>
        InProgress = 1,
        /// <summary>
        /// 计划暂停中
        /// </summary>
        Paused = 2,
        /// <summary>
        /// 计划已经完成
        /// </summary>
        Completed = 3,
        /// <summary>
        /// 计划被取消
        /// </summary>
        Canceled = 4,
    }
}
