using DnsClient;
using DnsClient.Protocol;
using System.Collections.Concurrent;
using UZonMail.Utils.Web.Service;

namespace UZonMail.ProPlugin.Services.EmailVerify
{
    public class MxRecords
    {
        public string Domain { get; set; }

        public List<string> Records { get; set; } = [];

        /// <summary>
        /// 是否有效
        /// </summary>
        public bool Valid { get; set; }

        public string GetRandomRecord()
        {
            if (Records.Count == 0 || !Valid) return string.Empty;

            // 随机返回
            var randomIndex = new Random().Next(0, Records.Count);
            return Records[randomIndex];
        }
    }

    /// <summary>
    /// MX 记录管理器
    /// </summary>
    public class MxManager : IScopedService
    {
        private ConcurrentDictionary<string, MxRecords> _mxRecords = [];

        /// <summary>
        /// 获取随机的 MX 记录
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public async Task<string> GetRandomMxRecord(string domain)
        {
            if (_mxRecords.TryGetValue(domain, out var value))
            {
                return value.GetRandomRecord();
            }

            var client = new LookupClient();
            var dnsResult = await client.QueryAsync(domain, QueryType.MX);
            var mxs = dnsResult.Answers.MxRecords().ToList() ?? [];

            // 添加记录
            var record = new MxRecords
            {
                Domain = domain,
                Valid = mxs.Count > 0,
                Records = [.. mxs.Select(x => x.Exchange.Value)]
            };
            _mxRecords.TryAdd(domain, record);
            return record.GetRandomRecord();
        }

        public void MarkInvalid(string mxRecordOrDomain)
        {
            if (_mxRecords.TryGetValue(mxRecordOrDomain, out var value))
            {
                value.Valid = false;
                return;
            }

            foreach(var valueTemp in _mxRecords.Values)
            {
                if (valueTemp.Records.Contains(mxRecordOrDomain))
                {
                    valueTemp.Valid = false;
                    return;
                }
            }
        }
    }
}
