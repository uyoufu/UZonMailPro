using log4net;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using UZonMail.Core.Services.SendCore.DynamicProxy.Clients;
using UZonMail.Core.Services.SendCore.DynamicProxy.ProxyTesters;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Utils.Json;
using UZonMailProPlugin.Services.ProxyFactories.YDaili;

namespace UZonMailProPlugin.Services.ProxyFactories.Ip2World
{
    public class Ip2WorldProxyClient : ProxyHandlerCluster
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(YDailiProxyClient));

        private int _ipNumber = 5;

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

            var (json, response) = await new Ip2WorldGetter(ProxyInfo.Url)
                .WithJsonReturnType()
                .WithDelimiter()
                .WithSocks5()
                .WithIPNumber(_ipNumber)
                .WithHttpClient(httpClient)
                .GetJsonAsync2();

            var status = json.SelectTokenOrDefault("success", false);
            if (!status)
            {
                _isAvailable = false;
                _logger.Error($"代理 {Id} 请求错误: {json.SelectTokenOrDefault("msg", "无法获取代理 ip")}");
            }

            var ipList = json!.SelectTokenOrDefault<List<JObject>>("data", []);
            // 将 IP 转换成代理客户端
            var handlers = ipList!.Select(x =>
                {
                    var ip = x.SelectTokenOrDefault("ip", string.Empty);
                    var port = x.SelectTokenOrDefault("port", string.Empty);
                    return $"{ip}:{port}";
                })
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => new Proxy()
                {
                    // ip 当做 Id
                    ObjectId = x,
                    Url = $"socks5://{x}",
                })
                .Select(x =>
                {
                    var handler = serviceProvider.GetRequiredService<ProxyHandler>();
                    // 提取 url 中的 expireMinutes 参数
                    var expireSeconds = GetExpireMinutes(x.Url) * 60;
                    handler.Update(x, ProxyTesterType.Google, expireSeconds);
                    return handler;
                })
                .ToList();

            return handlers;
        }
    }
}
