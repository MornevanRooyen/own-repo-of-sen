using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Sen381Backend.Models
{
    [Table("user_events")] // Supabase table name
    public class UserEvent : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("when_at")]
        public DateTime WhenAt { get; set; }

        [Column("duration_minutes")]
        public int DurationMinutes { get; set; } = 60;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}


