using Newtonsoft.Json.Linq;
using System.Net.Http;
using UZonMailProPlugin.Services.Crawlers.ByteDance.APIs;

namespace UZonMailProPlugin.Services.Crawlers.TikTok
{
    public abstract class RemoteItemListGetter
    {
        private List<JObject> _tempValues = [];
        private int _index = 0;

        private int _totalIndex = -1;

        /// <summary>
        /// 总的粉丝数量
        /// </summary>
        private int _total { get; set; }

        /// <summary>
        /// 为空时，表示没有粉丝了
        /// </summary>
        /// <returns></returns>
        public async Task<JObject?> Next()
        {
            if (_totalIndex >= _total)
            {
                return null;
            }

            // 拉取下一页
            await PullNextPageCore();

            _totalIndex++;
            if (_totalIndex >= _total || _tempValues.Count == 0)
            {
                return null;
            }

            var result = _tempValues[_index];
            _index++;

            return result;
        }

        public virtual void Reset()
        {
            _index = 0;
            _tempValues.Clear();
            _totalIndex = -1;
            _total = 0;
        }

        private async Task PullNextPageCore()
        {
            if (_index < _tempValues.Count)
            {
                return;
            }

            await PullNextPage();
        }

        protected abstract Task PullNextPage();

        protected void UpdatePage(int total, List<JObject> data)
        {
            _total = total;
            _tempValues.Clear();
            _tempValues.AddRange(data);

            _index = 0;
        }
    }
}
