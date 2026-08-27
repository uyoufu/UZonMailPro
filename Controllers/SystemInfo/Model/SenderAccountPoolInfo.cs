using UzonMail.CorePlugin.Services.SendCore.SenderAccounts;

namespace UzonMail.Pro.Controllers.SystemInfo.Model
{
    public class SenderAccountPoolInfo
    {
        public long UserId { get; private set; }
        public int SenderAccountCount { get; private set; }

        public SenderAccountPoolInfo(long userId, int userSenderAccountCount)
        {
            UserId = userId;
            SenderAccountCount = userSenderAccountCount;
        }
    }
}
