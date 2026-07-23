using UZonMail.CorePlugin.Services.SendCore.WaitList;

namespace UZonMail.Pro.Controllers.SystemInfo.Model
{
    public class SendingGroupInfo
    {
        public long UserId { get; private set; }
        public int SendingGroupsCount { get; private set; }

        public SendingGroupInfo(UserGroupTasksPool sendingGroupsPool)
        {
            UserId = sendingGroupsPool.UserId;
            SendingGroupsCount = sendingGroupsPool.Count;
        }
    }
}
