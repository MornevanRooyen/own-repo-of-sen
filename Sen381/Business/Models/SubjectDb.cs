using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Sen381.Business.Models
{
    [Table("subjects")]
    public class SubjectDb : BaseModel
    {
        [PrimaryKey("subject_id", true)]
        [Column("subject_id", ignoreOnInsert: true)]
        public int SubjectId { get; set; }

        [Column("subject_code")]
        public string SubjectCode { get; set; } = "";

        [Column("name")]
        public string Name { get; set; } = "";

        [Column("description")]
        public string? Description { get; set; }

        [Column("year")]
        public int Year { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
