using Microsoft.AspNetCore.Mvc;
using Sen381.Business.Models;
using Sen381.Data_Access;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace Sen381Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly SupaBaseAuthService _supabase;

        public NotificationsController(SupaBaseAuthService supabase)
        {
            _supabase = supabase;
        }

        // =============================
        // GET: Get all notifications for a user
        // =============================
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserNotifications(int userId)
        {
            Console.WriteLine($"[NotificationsController] Getting notifications for user {userId}");

            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var response = await client
                    .From<Notification>()
                    .Select("*")
                    .Filter("user_id", Operator.Equals, userId)
                    .Order("sent_at", Ordering.Descending)
                    .Limit(20) // Get latest 20 notifications
                    .Get();

                // Map to DTO to avoid serialization issues
                var notifications = response.Models.Select(n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    UserId = n.UserId,
                    Type = n.Type,
                    SubjectId = n.SubjectId,
                    Title = n.Title,
                    Body = n.Body,
                    SentAt = n.SentAt,
                    Priority = n.Priority,
                    IsRead = n.IsRead
                }).ToList();

                Console.WriteLine($"✅ Found {notifications.Count} notifications for user {userId}");
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error getting notifications: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // =============================
        // GET: Get unread notifications count
        // =============================
        [HttpGet("unread-count/{userId}")]
        public async Task<IActionResult> GetUnreadCount(int userId)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var response = await client
                    .From<Notification>()
                    .Select("*")
                    .Filter("user_id", Operator.Equals, userId)
                    .Get();

                // Filter unread client-side
                int count = response.Models.Count(n => !n.IsRead);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error getting unread count: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // =============================
        // PUT: Mark notification as read
        // =============================
        [HttpPut("mark-read/{notificationId}")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var response = await client
                    .From<Notification>()
                    .Select("*")
                    .Filter("notification_id", Operator.Equals, notificationId)
                    .Get();

                var notification = response.Models.FirstOrDefault();
                if (notification == null)
                {
                    return NotFound(new { error = "Notification not found" });
                }

                notification.IsRead = true;
                await client
                    .From<Notification>()
                    .Update(notification);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error marking notification as read: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // =============================
        // PUT: Mark all notifications as read for a user
        // =============================
        [HttpPut("mark-all-read/{userId}")]
        public async Task<IActionResult> MarkAllAsRead(int userId)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var response = await client
                    .From<Notification>()
                    .Select("*")
                    .Filter("user_id", Operator.Equals, userId)
                    .Get();

                // Filter unread notifications client-side
                var unreadNotifications = response.Models.Where(n => !n.IsRead).ToList();

                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                    await client
                        .From<Notification>()
                        .Update(notification);
                }

                Console.WriteLine($"✅ Marked {unreadNotifications.Count} notifications as read for user {userId}");
                return Ok(new { success = true, count = unreadNotifications.Count });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error marking all notifications as read: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; }
        public int? SubjectId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime SentAt { get; set; }
        public string Priority { get; set; }
        public bool IsRead { get; set; }
    }
}

