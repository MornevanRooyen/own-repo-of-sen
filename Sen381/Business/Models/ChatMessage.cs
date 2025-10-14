using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Sen381.Business.Models
{
    [Table("chat_messages")]
    public class ChatMessage : BaseModel
    {
        [PrimaryKey("chat_message_id", false)]
        [Column("chat_message_id")]
        public int ChatMessageId { get; set; }

        [Column("session_id")]
        public int SessionId { get; set; }

        [Column("sender_user_id")]
        public int SenderUserId { get; set; }

        [Column("text")]
        public string Text { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("image_file_id")]
        public int? ImageFileId { get; set; }
    }
}
