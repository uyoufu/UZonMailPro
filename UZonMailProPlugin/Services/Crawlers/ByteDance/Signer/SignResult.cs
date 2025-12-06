namespace UZonMail.ProPlugin.Modules.ByteDance.Signer
{
    public abstract class SignResult(string requestUrl,string msToken, string signature)
    {
        public string MsToken { get; set; } = msToken;
        public string Signature { get; set; } = signature;
        public string Bogus { get; set; }

        private string _originUrl = requestUrl;

        public string GetMsTokenUrl()
        {
            return $"{_originUrl}&msToken={MsToken}";
        }

        public abstract string GetFinnalUrl();
    }
}
