namespace Frontend.Models
{
    public class SignalRMessageDto
    {
        public int MessageId { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string MessageText { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public string SenderName { get; set; } = string.Empty;
    }
}
