using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Sen381.Business.Models
{
    [Table("user_follows")]
    public class UserFollow : BaseModel
    {
        [PrimaryKey("id", true)]
        [Column("id", ignoreOnInsert: true)]
        public int Id { get; set; }

        [Column("follower_id")]
        public int FollowerId { get; set; }

        [Column("following_id")]
        public int FollowingId { get; set; }

        [Column("created_at", ignoreOnInsert: true, ignoreOnUpdate: true)]
        public DateTime? CreatedAt { get; set; }
    }
}

