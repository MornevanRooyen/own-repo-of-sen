using System;
using System.Linq;
using System.Threading.Tasks;
using Supabase;
using Sen381.Business.Models;
using Sen381.Data_Access;

namespace Sen381.Business.Services
{
    public class NotificationService
    {
        private readonly SupaBaseAuthService _supabase;

        public NotificationService(SupaBaseAuthService supabase)
        {
            _supabase = supabase;
        }

        /// <summary>
        /// Creates a new notification record in the database.
        /// </summary>
        public async Task CreateNotificationAsync(Notification notification)
        {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));

            await _supabase.InitializeAsync();
            var client = _supabase.Client;

            notification.SentAt = DateTime.UtcNow;
            notification.IsRead = false;

            await client.From<Notification>().Insert(notification);
        }

        /// <summary>
        /// Gets all notifications for a user.
        /// </summary>
        public async Task<IQueryable<Notification>> GetNotificationsAsync(int userId)
        {
            await _supabase.InitializeAsync();
            var client = _supabase.Client;

            var response = await client.From<Notification>().Get();
            return response.Models.AsQueryable().Where(n => n.UserId == userId);
        }

        /// <summary>
        /// Marks a notification as read.
        /// </summary>
        public async Task MarkAsReadAsync(int notificationId)
        {
            await _supabase.InitializeAsync();
            var client = _supabase.Client;

            var response = await client.From<Notification>().Get();
            var notification = response.Models.FirstOrDefault(n => n.NotificationId == notificationId);

            if (notification != null)
            {
                notification.IsRead = true;
                await client.From<Notification>().Update(notification);
            }
        }
    }
}
