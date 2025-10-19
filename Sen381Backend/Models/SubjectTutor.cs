using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Sen381Backend.Models
{
    [Table("subject_tutors")]
    public class SubjectTutor : BaseModel
    {
        [PrimaryKey("subject_tutor_id", true)]
        [Column("subject_tutor_id", ignoreOnInsert: true)]
        public int SubjectTutorId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("subject_id")]
        public int SubjectId { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("approved_at", ignoreOnInsert: true, ignoreOnUpdate: true)]
        public DateTime? ApprovedAt { get; set; } = DateTime.UtcNow;

        [Column("approved_by")]
        public int? ApprovedBy { get; set; }

        [Column("created_at", ignoreOnInsert: true, ignoreOnUpdate: true)]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class SubjectTutorDto
    {
        public int SubjectTutorId { get; set; }
        public int UserId { get; set; }
        public int SubjectId { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        
        // Additional fields from joined tables
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ProfilePicturePath { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int SubjectYear { get; set; }
        public bool Following { get; set; } = false;
    }
}
