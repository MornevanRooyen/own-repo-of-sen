using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Sen381.Business.Models
{
    [Table("direct_messages")]
    public class DirectMessage : BaseModel
    {
        [PrimaryKey("id", true)]
        [Column("id", ignoreOnInsert: true)]
        public int Id { get; set; }

        [Column("sender_id")]
        public int SenderId { get; set; }

        [Column("receiver_id")]
        public int ReceiverId { get; set; }

        [Column("message_text")]
        public string MessageText { get; set; }

        [Column("sent_at", ignoreOnInsert: true, ignoreOnUpdate: true)]
        public DateTime? SentAt { get; set; }

        [Column("is_read")]
        public bool IsRead { get; set; }
    }
}

