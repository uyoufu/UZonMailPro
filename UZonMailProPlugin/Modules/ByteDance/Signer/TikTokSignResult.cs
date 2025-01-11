namespace UZonMailProPlugin.Modules.ByteDance.Signer
{
    public class TikTokSignResult(string requestUrl, string msToken, string signature) : SignResult(requestUrl, msToken, signature)
    {
        public override string GetFinnalUrl()
        {
            return $"{GetMsTokenUrl()}&_signature={Signature}&X-Bogus={Bogus}";
        }
    }
}
