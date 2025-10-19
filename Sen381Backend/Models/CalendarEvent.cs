using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Sen381Backend.Models
{
    [Table("calendar_events")]
    public class CalendarEvent : BaseModel
    {
        [PrimaryKey("event_id", true)]
        [Column("event_id", ignoreOnInsert: true)]
        public int EventId { get; set; }

        [Column("booking_id")]
        public int? BookingId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("start_time")]
        public DateTime StartTime { get; set; }

        [Column("end_time")]
        public DateTime EndTime { get; set; }

        [Column("event_type")]
        public string EventType { get; set; } = "booking";

        [Column("is_all_day")]
        public bool IsAllDay { get; set; } = false;

        [Column("created_at", ignoreOnInsert: true, ignoreOnUpdate: true)]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at", ignoreOnInsert: true, ignoreOnUpdate: true)]
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class CalendarEventDto
    {
        public int EventId { get; set; }
        public int? BookingId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string EventType { get; set; } = string.Empty;
        public bool IsAllDay { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Additional fields from joined tables
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int? SubjectId { get; set; }
        public string? BookingStatus { get; set; }
        public string? SubjectCode { get; set; }
        public string? SubjectName { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}
