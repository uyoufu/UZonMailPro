using System.Text.RegularExpressions;
using UZonMailProPlugin.SQL.EmailCrawler;

namespace UZonMailProPlugin.Services.Crawlers.TikTok
{
    public class SignatureResolver(string signature)
    {
        /// <summary>
        /// 解析签名
        /// </summary>
        /// <param name="signature"></param>
        /// <returns></returns>
        public void ResolveFor(TiktokAuthor tiktokAuthor)
        {
            if (string.IsNullOrEmpty(signature)) return;

            tiktokAuthor.Email = ResolveEmail();
            tiktokAuthor.Phone = ResolvePhone();
            tiktokAuthor.WhatsApp = ResolveWhatsApp();
            tiktokAuthor.Instagram = ResolveInstagram();
            tiktokAuthor.Youtube = ResolveYoutube();

            tiktokAuthor.IsParsed = !(string.IsNullOrEmpty(tiktokAuthor.Email)
                 && string.IsNullOrEmpty(tiktokAuthor.Phone)
                 && string.IsNullOrEmpty(tiktokAuthor.WhatsApp)
                 && string.IsNullOrEmpty(tiktokAuthor.Instagram)
                 && string.IsNullOrEmpty(tiktokAuthor.Youtube));
        }



        private string MatchGroupValue(string pattern)
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            var match = regex.Match(signature);
            if (!match.Success) return string.Empty;
            return match.Groups[1].Value;
        }

        private string GetMatchValue(string pattern)
        {
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            var match = regex.Match(signature);
            if (!match.Success) return string.Empty;
            return match.Value;
        }

        private string ResolveEmail()
        {
            return MatchGroupValue(@"([a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,})");
        }

        private string ResolvePhone()
        {
            // 使用正则表达式解析电话号码
            var phone1 = MatchGroupValue(@"\b(\d{3}-\d{6})\b");
            if (!string.IsNullOrEmpty(phone1)) return phone1;

            var phone2 = MatchGroupValue(@"\b(\d{9,12})\b");
            if (!string.IsNullOrEmpty(phone2)) return phone2;

            var phone3 = MatchGroupValue(@"\b(\d{10})\b");
            if (!string.IsNullOrEmpty(phone3)) return phone3;

            var phone4 = MatchGroupValue(@"Cell\s+(\d{11})");
            if (!string.IsNullOrEmpty(phone4)) return phone4;

            var phone5 = MatchGroupValue(@"Call\s+(\d{11})");
            return phone5;
        }

        private string ResolveWhatsApp()
        {
            var whatsApp1 = MatchGroupValue(@"WhatsApp[\s+\-\+:]([0-9]+)");
            if (!string.IsNullOrEmpty(whatsApp1)) return whatsApp1;

            var whatsApp2 = MatchGroupValue(@"https:\/\/wa\.me/(\d+)");
            return whatsApp2;
        }

        private string ResolveInstagram()
        {
            return MatchGroupValue(@"Instagram\s*[\s+:\-]\s*(\w+)");
        }

        private string ResolveYoutube()
        {
            var youtue1 = GetMatchValue(@"https?://(?:www\.)?youtube\.com/(?:[^\s/]*/)+([^\s/?]+)");
            if (!string.IsNullOrEmpty(youtue1)) return youtue1;

            var youtube2 = GetMatchValue(@"https?://(?:www\.)?youtube\.com/(?:channel|user)/[a-zA-Z0-9-_]+");
            return youtube2;
        }
    }
}
