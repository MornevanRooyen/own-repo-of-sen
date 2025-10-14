using Microsoft.AspNetCore.Mvc;
using Sen381.Business.Models;
using Sen381.Business.Services;
using Sen381.Data_Access;
using Supabase;
using Supabase.Postgrest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sen381Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly Client _client;
        private readonly NotificationService _notifications;

        public ChatController(SupaBaseAuthService supabase, NotificationService notifications)
        {
            _client = supabase.Client; // ✅ Use pre-initialized service client (service role key)
            _notifications = notifications;
        }

        // ========================================
        // 📩 SEND MESSAGE (STUDENT ↔ TUTOR)
        // ========================================
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessage message)
        {
            if (message == null || string.IsNullOrWhiteSpace(message.Text))
                return BadRequest(new { error = "Message text cannot be empty." });

            // Ensure session exists
            var sessionResp = await _client
                .From<ChatSession>()
                .Filter("chat_session_id", Supabase.Postgrest.Constants.Operator.Equals, message.SessionId)
                .Get();

            var session = sessionResp.Models.FirstOrDefault();
            if (session == null)
                return NotFound(new { error = "Chat session not found." });

            // Insert message
            message.CreatedAt = DateTime.UtcNow;
            await _client.From<ChatMessage>().Insert(message);

            // Determine receiver
            int receiverId = (message.SenderUserId == session.StudentId)
                ? session.TutorId
                : session.StudentId;

            // Send notification
            await _notifications.CreateNotificationAsync(new Notification
            {
                UserId = receiverId,
                Type = "chat_message",
                Title = "New Message",
                Body = $"You received a new message from user {message.SenderUserId}",
                SentAt = DateTime.UtcNow,
                IsRead = false
            });

            return Ok(new
            {
                message = "Message sent successfully.",
                sessionId = session.ChatSessionId
            });
        }

        // ========================================
        // 💬 GET CHAT HISTORY (ALL MESSAGES IN A SESSION)
        // ========================================
        [HttpGet("history/{sessionId}")]
        public async Task<IActionResult> GetChatHistory(int sessionId)
        {
            var response = await _client
                .From<ChatMessage>()
                .Filter("session_id", Supabase.Postgrest.Constants.Operator.Equals, sessionId)
                .Order("created_at", Supabase.Postgrest.Constants.Ordering.Ascending)
                .Get();

            return Ok(response.Models);
        }

        // ========================================
        // 🧑‍🤝‍🧑 GET USER SESSIONS (ALL CHAT SESSIONS FOR A USER)
        // ========================================
        [HttpGet("sessions/{userId}")]
        public async Task<IActionResult> GetUserSessions(int userId)
        {
            // 🔥 Manual filter approach (version-safe for all Supabase .NET SDKs)
            var studentSessions = await _client
                .From<ChatSession>()
                .Filter("student_id", Supabase.Postgrest.Constants.Operator.Equals, userId)
                .Get();

            var tutorSessions = await _client
                .From<ChatSession>()
                .Filter("tutor_id", Supabase.Postgrest.Constants.Operator.Equals, userId)
                .Get();

            // ✅ Combine both lists locally
            var allSessions = studentSessions.Models
                .Concat(tutorSessions.Models)
                .OrderByDescending(s => s.CreatedAt)
                .ToList();

            return Ok(allSessions);
        }




        // ========================================
        // ✅ CREATE NEW CHAT SESSION (IF NONE EXISTS)
        // ========================================
        [HttpPost("create-session")]
        public async Task<IActionResult> CreateSession([FromBody] ChatSession newSession)
        {
            if (newSession == null)
                return BadRequest(new { error = "Invalid chat session data." });

            // ✅ Correct Dictionary type (all string values)
            var matchFilters = new Dictionary<string, string>
            {
                { "student_id", newSession.StudentId.ToString() },
                { "tutor_id", newSession.TutorId.ToString() },
                { "subject_id", newSession.SubjectId.ToString() }
            };

            var existingResp = await _client
                .From<ChatSession>()
                .Match(matchFilters)
                .Get();

            var existing = existingResp.Models.FirstOrDefault();
            if (existing != null)
            {
                return Ok(new
                {
                    message = "Existing session found.",
                    sessionId = existing.ChatSessionId
                });
            }

            // Create new session
            newSession.Status = "open";
            newSession.CreatedAt = DateTime.UtcNow;
            await _client.From<ChatSession>().Insert(newSession);

            return Ok(new
            {
                message = "New chat session created successfully.",
                session = newSession
            });
        }

        // ========================================
        // ✅ MARK NOTIFICATIONS AS READ
        // ========================================
        [HttpPost("mark-read/{userId}")]
        public async Task<IActionResult> MarkNotificationsAsRead(int userId)
        {
            var unreadResp = await _client
                .From<Notification>()
                .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId)
                .Filter("is_read", Supabase.Postgrest.Constants.Operator.Equals, false)
                .Get();

            var unread = unreadResp.Models;
            if (unread.Count == 0)
                return Ok(new { message = "No unread notifications found." });

            foreach (var n in unread)
            {
                n.IsRead = true;
                await _client.From<Notification>().Update(n);
            }

            return Ok(new { message = "All notifications marked as read." });
        }
    }
}
