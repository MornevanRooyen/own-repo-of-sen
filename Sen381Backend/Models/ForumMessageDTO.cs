namespace Sen381Backend.Models
{
    public class ForumMessageDto
    {
        public int MessageId { get; set; }
        public int ForumId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsFile { get; set; } = false;
        public int? FileId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
