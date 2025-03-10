using UZonMail.DB.SQL.Core.EmailSending;

namespace UZonMailProPlugin.Controllers.Unsubscribes.ResponseModels
{
    /// <summary>
    /// 取消退订的 Payloads
    /// </summary>
    /// <param name="sendingItem"></param>
    public class UnsubscribePayloads(SendingItem sendingItem)
    {
        public long SendingItemId => sendingItem.Id;
        public string? Email => sendingItem.ToEmails;
        public long OrganizationId { get; set; }
    }
}
