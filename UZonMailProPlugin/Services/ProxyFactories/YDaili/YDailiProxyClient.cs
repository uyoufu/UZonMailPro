using log4net;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text.RegularExpressions;
using UZonMail.Core.Services.SendCore.Proxies.Clients;
using UZonMail.Core.Services.SendCore.Proxies.ProxyTesters;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Utils.Json;

namespace UZonMailProPlugin.Services.ProxyFactories.YDaili
{
    public class YDailiProxyClient : ProxyHandlersCluster
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(YDailiProxyClient));

        private int _ipNumber = 1;
        private int _maxNumber = 100;

        private bool _isAvailable = true;
        public override bool IsEnable()
        {
            return _isAvailable;
        }

        /// <summary>
        /// 若 IP 池为空时，父级会自动调用此方法获取新的 IP
        /// </summary>
        /// <returns></returns>
        protected override async Task<List<ProxyHandler>> GetProxyHandlersAsync(IServiceProvider serviceProvider)
        {
            var httpClient = serviceProvider.GetRequiredService<HttpClient>();

            var (json, response) = await new YDailiGetter(ProxyInfo.Url)
                .WithFormat(YDailiFormat.Json)
                .WithIPNumber(_ipNumber)
                .WithHttpClient(httpClient)
                .GetJsonAsync2();

            var status = json.SelectTokenOrDefault("status", "success");
            switch (status)
            {
                case "206":
                    _isAvailable = false;
                    _logger.Error($"代理 {Id} IP 数量已用完");
                    return [];

                case "210":
                    _isAvailable = false;
                    _logger.Error($"代理 {Id} 需要添加白名单");
                    return [];

                case "406":
                    _logger.Warn($"代理 {Id} 提取间隔太快");
                    // 增加单次提取数量
                    _ipNumber = Math.Min(_ipNumber + 10, _maxNumber);
                    await Task.Delay(2000);
                    return await GetProxyHandlersAsync(serviceProvider);

                case "215":
                    _logger.Warn($"代理 {Id} 单次提取数量超过上限");
                    // 减少单次提取数量
                    _ipNumber = Math.Max(_ipNumber - 10, 5);
                    return await GetProxyHandlersAsync(serviceProvider);

                default:
                    break;
            }

            var ipList = json!.SelectTokenOrDefault<List<JObject>>("data", []);
            // 将 IP 转换成代理客户端
            var handlers = ipList!.Select(x => x.SelectTokenOrDefault("IP", ""))
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => new Proxy()
                {
                    ObjectId = x,
                    Url = $"socks5://{x}",
                })
                .Select(x =>
                {
                    var handler = serviceProvider.GetRequiredService<ProxyHandler>();
                    // 提取 url 中的 expireMinutes 参数
                    var expireSeconds = GetExpireMinutes(x.Url) * 60;
                    handler.Update(x, ProxyZoneType.Baidu, expireSeconds);
                    return handler;
                })
                .ToList();

            return handlers;
        }
    }
}
