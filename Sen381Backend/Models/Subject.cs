using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Sen381Backend.Models
{
    [Table("subjects")]
    public class Subject : BaseModel
    {
        [PrimaryKey("subject_id", true)]
        [Column("subject_id", ignoreOnInsert: true)]
        public int SubjectId { get; set; }

        [Column("subject_code")]
        public string SubjectCode { get; set; } = string.Empty;

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("year")]
        public int Year { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at", ignoreOnInsert: true, ignoreOnUpdate: true)]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at", ignoreOnInsert: true, ignoreOnUpdate: true)]
        public DateTime? UpdatedAt { get; set; }
    }
}
