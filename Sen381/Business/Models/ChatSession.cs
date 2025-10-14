using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Sen381.Business.Models
{
    [Table("chat_sessions")]
    public class ChatSession : BaseModel
    {
        [PrimaryKey("chat_session_id", false)]
        [Column("chat_session_id")]
        public int ChatSessionId { get; set; }

        [Column("student_id")]
        public int StudentId { get; set; }

        [Column("tutor_id")]
        public int TutorId { get; set; }

        [Column("subject_id")]
        public int? SubjectId { get; set; }

        [Column("status")]
        public string Status { get; set; } = "open";

        [Column("escalation_id")]
        public int? EscalationId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
