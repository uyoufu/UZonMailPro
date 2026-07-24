using UzonMail.CorePlugin.Services.SendCore.Outboxes;

namespace UzonMail.Pro.Controllers.SystemInfo.Model
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
