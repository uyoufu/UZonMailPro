using UZonMail.CorePlugin.Services.SendCore.Outboxes;

namespace UZonMail.Pro.Controllers.SystemInfo.Model
{
    public class OutboxPoolInfo
    {
        public long UserId { get; private set; }
        public int OutboxesCount { get; private set; }

        public OutboxPoolInfo(long userId, int userOutboxesCount)
        {
            UserId = userId;
            OutboxesCount = userOutboxesCount;
        }
    }
}
