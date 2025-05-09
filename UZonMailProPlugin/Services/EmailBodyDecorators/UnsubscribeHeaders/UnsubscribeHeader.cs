namespace UZonMailProPlugin.Services.EmailBodyDecorators.UnsubscribeHeaders
{
    public class UnsubscribeConfig
    {
        public List<UnsubscribeHeader> Headers { get; set; } = [];
    }

    /// <summary>
    /// Header 配置
    /// </summary>
    public class UnsubscribeHeader
    {
        public string Domain { get; set; }
        public string Header { get; set; }
        public string Description { get; set; }
    }
}
