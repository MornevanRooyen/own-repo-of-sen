using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Sen381.Business.Models
{
    [Table("topics")]
    public class TopicDb : BaseModel
    {
        [PrimaryKey("topic_id", true)]
        [Column("topic_id", ignoreOnInsert: true)]
        public int TopicId { get; set; }

        [Column("subject_id")]
        public int SubjectId { get; set; }

        [Column("title")]
        public string Title { get; set; } = "";

        [Column("description")]
        public string? Description { get; set; }

        [Column("order_number")]
        public int OrderNumber { get; set; } = 1;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
