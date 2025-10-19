using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Sen381Backend.Models
{
    [Table("subject_subscriptions")]
    public class SubjectSubscription : BaseModel
    {
        [PrimaryKey("subscription_id", true)]
        [Column("subscription_id", ignoreOnInsert: true)]
        public int SubscriptionId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("subject_id")]
        public int SubjectId { get; set; }

        [Column("subscribed_at", ignoreOnInsert: true, ignoreOnUpdate: true)]
        public DateTime? SubscribedAt { get; set; } = DateTime.UtcNow;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }

    public class SubjectSubscriptionDto
    {
        public int SubscriptionId { get; set; }
        public int UserId { get; set; }
        public int SubjectId { get; set; }
        public DateTime? SubscribedAt { get; set; }
        public bool IsActive { get; set; }
        
        // Additional fields from joined tables
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ProfilePicturePath { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int SubjectYear { get; set; }
    }

    public class SubscriptionRequest
    {
        public int UserId { get; set; }
        public int SubjectId { get; set; }
    }

    public class SubscriptionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsSubscribed { get; set; }
    }
}
