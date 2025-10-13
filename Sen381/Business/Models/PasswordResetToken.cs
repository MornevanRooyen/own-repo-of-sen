using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sen381.Business.Models
{
    public class PasswordResetToken
    {
        // ---------- Fields ----------
        private int id;
        private string tokenHash;
        private int userId;

        // ---------- Properties ----------
        public int Id
        {
            get => id;
            set => id = value;
        }

        public int UserId
        {
            get => userId;
            set => userId = value;
        }

        public DateTime ExpiresAt { get; set; }

        // ---------- Constructor ----------
        public PasswordResetToken(int userId, string tokenHash, DateTime expiresAt)
        {
            this.userId = userId;
            this.tokenHash = tokenHash;
            ExpiresAt = expiresAt;
        }

        // ---------- Methods ----------
        public void MarkUsed()
        {
            // In a real system, you’d flag the token as consumed
            Console.WriteLine("Password reset token marked as used.");
        }

        public void Revoke()
        {
            // Invalidate immediately by expiring now
            ExpiresAt = DateTime.Now;
            Console.WriteLine("Password reset token revoked.");
        }

        public TimeSpan RemainingTime()
        {
            return ExpiresAt - DateTime.Now;
        }
    }
}
