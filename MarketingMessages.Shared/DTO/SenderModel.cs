using MarketingMessages.Shared.Models;

namespace MarketingMessages.Shared.DTO
{
    public class SenderModel
    {
        public int SenderId { get; set; }
        public string SenderName { get; set; } = "";
        public string FromEmail { get; set; } = "";
        public string ReplyTo { get; set; } = "";

        public SenderModel()
        {

        }
        public SenderModel(Sender sender)
        {
            SenderId = sender.SenderId;
            SenderName = sender.Name;
            FromEmail = sender.Email;
            ReplyTo = sender.ReplyTo;
        }
    }
}