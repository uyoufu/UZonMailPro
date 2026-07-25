namespace UzonMail.ProPlugin.Services.EmailBodyDecorators.UnsubscribeHeaders
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
        public string Domain { get; set; } = string.Empty;
        public string Header { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
