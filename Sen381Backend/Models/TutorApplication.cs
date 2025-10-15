using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Sen381Backend.Models
{
    [Table("tutor_applications")]
    public class TutorApplication : BaseModel
    {
        [PrimaryKey("application_id", true)]
        [Column("application_id", ignoreOnInsert: true)]
        public int ApplicationId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [Column("last_name")]
        public string LastName { get; set; } = string.Empty;

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("phone_num")]
        public string? PhoneNum { get; set; }

        [Column("student_no")]
        public string? StudentNo { get; set; }

        [Column("major")]
        public string? Major { get; set; }

        [Column("year_of_study")]
        public int? YearOfStudy { get; set; }

        [Column("completed_sessions")]
        public int CompletedSessions { get; set; } = 0;

        [Column("min_required_grade")]
        public int? MinRequiredGrade { get; set; }

        [Column("profile_picture_path")]
        public string? ProfilePicturePath { get; set; }

        [Column("transcript_path")]
        public string? TranscriptPath { get; set; }

        [Column("status")]
        public string Status { get; set; } = "pending"; // pending, approved, declined

        [Column("created_at", ignoreOnInsert: true, ignoreOnUpdate: true)]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("reviewed_at")]
        public DateTime? ReviewedAt { get; set; }

        [Column("reviewed_by")]
        public int? ReviewedBy { get; set; } // Admin user ID who reviewed

        [Column("review_notes")]
        public string? ReviewNotes { get; set; }
    }

    public class TutorApplicationInput
    {
        public int UserId { get; set; }
        public string? PhoneNum { get; set; }
        public string? StudentNo { get; set; }
        public string? Major { get; set; }
        public int? YearOfStudy { get; set; }
        public int? MinRequiredGrade { get; set; }
        public string? TranscriptPath { get; set; }
    }

    public class TutorApplicationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? ApplicationId { get; set; }
    }
}
