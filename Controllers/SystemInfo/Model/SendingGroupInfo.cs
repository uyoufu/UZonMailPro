namespace UzonMail.Pro.Controllers.SystemInfo.Model
{
    public class SendingGroupInfo
    {
        public long UserId { get; private set; }
        public int SendingGroupsCount { get; private set; }

        public SendingGroupInfo(long userId, int sendingGroupsCount)
        {
            UserId = userId;
            SendingGroupsCount = sendingGroupsCount;
        }
    }
}
