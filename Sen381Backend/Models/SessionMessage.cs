using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Sen381Backend.Models
{
    [Table("session_messages")]
    public class SessionMessage : BaseModel
    {
        [PrimaryKey("message_id")]
        public int MessageId { get; set; }

        [Column("session_id")]
        public int SessionId { get; set; }

        [Column("sender_id")]
        public int SenderId { get; set; }

        [Column("sender_name")]
        public string SenderName { get; set; } = string.Empty;

        [Column("content")]
        public string Content { get; set; } = string.Empty;

        [Column("is_file")]
        public bool IsFile { get; set; } = false;

        [Column("file_id")]
        public int? FileId { get; set; }

        [Column("created_at", ignoreOnInsert: true, ignoreOnUpdate: true)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // DTO for API responses (without Supabase attributes)
    public class SessionMessageDto
    {
        public int MessageId { get; set; }
        public int SessionId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsFile { get; set; } = false;
        public int? FileId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}