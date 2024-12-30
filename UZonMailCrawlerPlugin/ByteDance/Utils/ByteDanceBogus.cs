using System.Text;
using System;

namespace UZonMailCrawlerPlugin.ByteDance.Utils
{
    /// <summary>
    /// 生成 Bogous
    /// </summary>
    public abstract class ByteDanceBogus
    {
        /// <summary>
        /// 生成 msToken
        /// </summary>
        /// <param name="randomLength"></param>
        /// <returns></returns>
        public string GetMsToken(int randomLength = 107)
        {
            const string baseStr = "ABCDEFGHIGKLMNOPQRSTUVWXYZabcdefghigklmnopqrstuvwxyz0123456789=";
            int length = baseStr.Length;
            StringBuilder randomStr = new(randomLength);
            for (int i = 0; i < randomLength; i++)
            {
                var index = new Random().Next(0, length);
                randomStr.Append(baseStr[index]);
            }
            return randomStr.ToString();
        }

        protected string GetSignature()
        {
            return "";
        }

        /// <summary>
        /// 获取 Bogus
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <returns></returns>
        public abstract string GetBogus(string requestUrl);
    }
}
