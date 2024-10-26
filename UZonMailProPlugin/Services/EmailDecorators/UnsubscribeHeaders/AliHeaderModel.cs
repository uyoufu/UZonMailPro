using Newtonsoft.Json;

namespace UZonMailProPlugin.Services.EmailDecorators.UnsubscribeHeaders
{
    public class AliHeaderModel
    {
        public string Version { get; set; } = "1.0";
        public AliUnsusbscribeInfo Unsusbscribe { get; set; } = new AliUnsusbscribeInfo();

        private AliHeaderModel() { }

        public static string GetHeaderValue()
        {
            var header = new AliHeaderModel();
            var headerJson = JsonConvert.SerializeObject(header);
            // 转换成 Base64
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(headerJson));
        }
    }

    public class AliUnsusbscribeInfo
    {
        public string LinkType { get; set; } = "default";
        public string FilterLevel { get; set; } = "mailfrom_domain";
    }
}
