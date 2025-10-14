using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Sen381.Business.Models
{
    [Table("notifications")]
    public class Notification : BaseModel
    {
        [PrimaryKey("notification_id", true)]
        [Column("notification_id", ignoreOnInsert: true)]
        public int NotificationId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("subject_id")]
        public int? SubjectId { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("body")]
        public string Body { get; set; }

        [Column("sent_at")]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        [Column("priority")]
        public string Priority { get; set; } = "normal";

        [Column("is_read")]
        public bool IsRead { get; set; } = false;
    }
}
