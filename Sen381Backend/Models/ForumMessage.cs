using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Sen381Backend.Models
{
    [Table("forum_messages")]
    public class ForumMessage : BaseModel
    {
        [PrimaryKey("message_id")]
        public int MessageId { get; set; }

        [Column("forum_id")]
        public int ForumId { get; set; }

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
}
