using UZonMail.Core.Services.SendCore.Outboxes;

namespace UZonMail.Pro.Controllers.SystemInfo.Model
{
    public class OutboxPoolInfo
    {
        public long UserId { get; private set; }
        public int OutboxesCount { get; private set; }

        public OutboxPoolInfo(OutboxesPool userOutboxPool)
        {
            UserId = userOutboxPool.UserId;
            OutboxesCount = userOutboxPool.Count;
        }
    }
}
