using System;
using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Sen381Backend.Models
{
    [Table("booking_sessions")]
    public class BookingSession : BaseModel
    {
        [PrimaryKey("booking_id", true)]
        [Column("booking_id", ignoreOnInsert: true)]
        public int BookingId { get; set; }

        [Column("tutor_id")]
        public int TutorId { get; set; }

        [Column("student_id")]
        public int StudentId { get; set; }

        [Column("subject_id")]
        public int SubjectId { get; set; }

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("session_date")]
        public DateTime SessionDate { get; set; }

        [Column("duration_minutes")]
        public int DurationMinutes { get; set; }

        [Column("status")]
        public string Status { get; set; } = "pending";

        [Column("created_at", ignoreOnInsert: true, ignoreOnUpdate: true)]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at", ignoreOnInsert: true, ignoreOnUpdate: true)]
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        [Column("completed_at")]
        public DateTime? CompletedAt { get; set; }
    }

    public class BookingSessionInput
    {
        [Required]
        public int TutorId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "Session title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Session date is required")]
        public DateTime SessionDate { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [Range(15, 480, ErrorMessage = "Duration must be between 15 minutes and 8 hours")]
        public int DurationMinutes { get; set; }
    }

    public class BookingSessionDto
    {
        public int BookingId { get; set; }
        public int TutorId { get; set; }
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime SessionDate { get; set; }
        public int DurationMinutes { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Additional fields from joined tables
        public string TutorFirstName { get; set; } = string.Empty;
        public string TutorLastName { get; set; } = string.Empty;
        public string TutorEmail { get; set; } = string.Empty;
        public string StudentFirstName { get; set; } = string.Empty;
        public string StudentLastName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int SubjectYear { get; set; }

        public string TutorFullName => $"{TutorFirstName} {TutorLastName}";
        public string StudentFullName => $"{StudentFirstName} {StudentLastName}";
        public DateTime EndDate => SessionDate.AddMinutes(DurationMinutes);
    }

            public class BookingResponse
            {
                public bool Success { get; set; }
                public string Message { get; set; } = string.Empty;
                public BookingSessionDto? Booking { get; set; }
            }

            // Separate model for inserts without BookingId
            [Table("booking_sessions")]
            public class BookingSessionInsert : BaseModel
            {
                [Column("tutor_id")]
                public int TutorId { get; set; }

                [Column("student_id")]
                public int StudentId { get; set; }

                [Column("subject_id")]
                public int SubjectId { get; set; }

                [Column("title")]
                public string Title { get; set; } = string.Empty;

                [Column("description")]
                public string? Description { get; set; }

                [Column("session_date")]
                public DateTime SessionDate { get; set; }

                [Column("duration_minutes")]
                public int DurationMinutes { get; set; }

                [Column("status")]
                public string Status { get; set; } = "pending";

                [Column("created_at", ignoreOnInsert: true, ignoreOnUpdate: true)]
                public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

                [Column("updated_at", ignoreOnInsert: true, ignoreOnUpdate: true)]
                public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
            }
}
