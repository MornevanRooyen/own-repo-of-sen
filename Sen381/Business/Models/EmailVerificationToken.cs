using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Sen381.Business.Models
{
    // ✅ Explicitly set schema to 'public'
    [Table("email_verification_tokens")]
    public class EmailVerificationToken : BaseModel
    {
        [PrimaryKey("email_verification_token_id", true)]
        [Column("email_verification_token_id", ignoreOnInsert: true)]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("token_hash")]
        public string TokenHash { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        // ✅ Check if the token has expired
        public bool IsExpired() => DateTime.UtcNow >= ExpiresAt;

        // ✅ Check if the token is still valid
        public bool IsValid() => !IsExpired();

        // ✅ Mark token as used (expire immediately)
        public void MarkUsed()
        {
            ExpiresAt = DateTime.UtcNow;
            Console.WriteLine("✅ Email verification token marked as used.");
        }

        // ✅ Revoke token manually
        public void Revoke()
        {
            ExpiresAt = DateTime.UtcNow;
            Console.WriteLine("⚠️ Email verification token revoked.");
        }

        // ✅ Get remaining time before expiration
        public TimeSpan RemainingTime()
        {
            var remaining = ExpiresAt - DateTime.UtcNow;
            return remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
        }
    }
}
