namespace UZonMailProPlugin.Services.EmailBodyDecorators.UnsubscribeHeaders
{
    public class UnsubscribeSettings
    {
        public List<UnsubscribeHeaderConfig> Headers { get; set; } = [];
    }

    /// <summary>
    /// Header 配置
    /// </summary>
    public class UnsubscribeHeaderConfig
    {
        public string Domain { get; set; }
        public string Header { get; set; }
        public string Description { get; set; }
    }
}
