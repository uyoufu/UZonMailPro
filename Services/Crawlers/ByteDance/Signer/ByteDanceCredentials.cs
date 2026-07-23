using System.Text;

namespace UZonMail.ProPlugin.Modules.ByteDance.Signer
{
    /// <summary>
    /// 生成 Bogous
    /// </summary>
    public abstract class ByteDanceCredentials
    {
        /// <summary>
        /// 生成 msToken
        /// </summary>
        /// <param name="randomLength"></param>
        /// <returns></returns>
        public static string GetMsToken(int randomLength = 107)
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

        /// <summary>
        /// 获取签名
        /// 常数即可
        /// </summary>
        /// <returns></returns>
        public static string GetSignature()
        {
            return "_02B4Z6wo000016M20awAAIDAnp.LMKuZmC-jNtUAAI6L17";
        }

        /// <summary>
        /// 获取 Bogus
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <returns></returns>
        public abstract string GetBogus(string requestUrl);
    }
}
