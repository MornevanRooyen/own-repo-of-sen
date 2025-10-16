using Microsoft.AspNetCore.Mvc;
using Sen381.Business.Models;
using Sen381.Business.Services;
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
    public class MessagingController : ControllerBase
    {
        private readonly SupaBaseAuthService _supabase;
        private readonly NotificationService _notificationService;

        public MessagingController(SupaBaseAuthService supabase, NotificationService notificationService)
        {
            _supabase = supabase;
            _notificationService = notificationService;
        }

        // =============================
        // POST: Send a direct message
        // =============================
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            Console.WriteLine($"[MessagingController] Sending message from {request.SenderId} to {request.ReceiverId}");

            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // Check if sender is following receiver
                var followCheck = await client
                    .From<UserFollow>()
                    .Select("*")
                    .Filter("follower_id", Operator.Equals, request.SenderId)
                    .Filter("following_id", Operator.Equals, request.ReceiverId)
                    .Get();

                if (!followCheck.Models.Any())
                {
                    return BadRequest(new { error = "You can only message users you follow" });
                }

                // Create the message
                var message = new DirectMessage
                {
                    SenderId = request.SenderId,
                    ReceiverId = request.ReceiverId,
                    MessageText = request.MessageText,
                    IsRead = false
                };

                var response = await client
                    .From<DirectMessage>()
                    .Insert(message);

                // Get sender's name for notification
                var senderResponse = await client
                    .From<User>()
                    .Select("*")
                    .Filter("user_id", Operator.Equals, request.SenderId)
                    .Get();

                var sender = senderResponse.Models.FirstOrDefault();
                string senderName = sender != null ? $"{sender.FirstName} {sender.LastName}" : "Someone";

                // Create notification for receiver (don't set NotificationId - let DB auto-generate it)
                var notification = new Notification
                {
                    UserId = request.ReceiverId,
                    Type = "direct_message",
                    Title = "New Message",
                    Body = $"<strong>{senderName}</strong> sent you a message",
                    Priority = "normal"
                    // NotificationId, SentAt, and IsRead will be set by database defaults
                };
                
                await _notificationService.CreateNotificationAsync(notification);

                Console.WriteLine($"✅ Message sent successfully with notification");
                return Ok(new { success = true, message = "Message sent successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error sending message: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // =============================
        // GET: Get conversation between two users
        // =============================
        [HttpGet("conversation/{userId1}/{userId2}")]
        public async Task<IActionResult> GetConversation(int userId1, int userId2)
        {
            Console.WriteLine($"[MessagingController] Getting conversation between {userId1} and {userId2}");

            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // Get messages from userId1 to userId2
                var response1 = await client
                    .From<DirectMessage>()
                    .Select("*")
                    .Filter("sender_id", Operator.Equals, userId1)
                    .Filter("receiver_id", Operator.Equals, userId2)
                    .Get();

                // Get messages from userId2 to userId1
                var response2 = await client
                    .From<DirectMessage>()
                    .Select("*")
                    .Filter("sender_id", Operator.Equals, userId2)
                    .Filter("receiver_id", Operator.Equals, userId1)
                    .Get();

                // Combine and sort by timestamp
                var allMessages = response1.Models.Concat(response2.Models)
                    .OrderBy(m => m.SentAt)
                    .Select(m => new MessageDto
                    {
                        Id = m.Id,
                        SenderId = m.SenderId,
                        ReceiverId = m.ReceiverId,
                        MessageText = m.MessageText,
                        SentAt = m.SentAt,
                        IsRead = m.IsRead
                    })
                    .ToList();

                Console.WriteLine($"✅ Found {allMessages.Count} messages in conversation");

                return Ok(allMessages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error getting conversation: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // =============================
        // GET: Get last message with each followed user
        // =============================
        [HttpGet("conversations/{userId}")]
        public async Task<IActionResult> GetAllConversations(int userId)
        {
            Console.WriteLine($"[MessagingController] Getting all conversations for user {userId}");

            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // Get all messages where this user is sender
                var sentResponse = await client
                    .From<DirectMessage>()
                    .Select("*")
                    .Filter("sender_id", Operator.Equals, userId)
                    .Get();

                // Get all messages where this user is receiver
                var receivedResponse = await client
                    .From<DirectMessage>()
                    .Select("*")
                    .Filter("receiver_id", Operator.Equals, userId)
                    .Get();

                // Combine and sort by timestamp descending
                var allMessages = sentResponse.Models.Concat(receivedResponse.Models)
                    .OrderByDescending(m => m.SentAt)
                    .ToList();
                
                // Group by conversation partner and get the latest message
                var conversations = new Dictionary<int, ConversationPreview>();

                foreach (var msg in allMessages)
                {
                    int partnerId = msg.SenderId == userId ? msg.ReceiverId : msg.SenderId;

                    if (!conversations.ContainsKey(partnerId))
                    {
                        conversations[partnerId] = new ConversationPreview
                        {
                            UserId = partnerId,
                            LastMessage = msg.MessageText,
                            LastMessageTime = msg.SentAt,
                            IsUnread = msg.ReceiverId == userId && !msg.IsRead
                        };
                    }
                }

                Console.WriteLine($"✅ Found {conversations.Count} conversations");
                return Ok(conversations.Values);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error getting conversations: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // =============================
        // PUT: Mark messages as read
        // =============================
        [HttpPut("mark-read/{senderId}/{receiverId}")]
        public async Task<IActionResult> MarkAsRead(int senderId, int receiverId)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // Get all messages from senderId to receiverId
                var messagesResponse = await client
                    .From<DirectMessage>()
                    .Select("*")
                    .Filter("sender_id", Operator.Equals, senderId)
                    .Filter("receiver_id", Operator.Equals, receiverId)
                    .Get();

                // Filter unread messages client-side and mark as read
                var unreadMessages = messagesResponse.Models.Where(m => !m.IsRead).ToList();
                
                foreach (var msg in unreadMessages)
                {
                    msg.IsRead = true;
                    await client
                        .From<DirectMessage>()
                        .Update(msg);
                }

                Console.WriteLine($"✅ Marked {unreadMessages.Count} messages as read");
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error marking messages as read: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // =============================
        // DELETE: Delete a message (only by sender)
        // =============================
        [HttpDelete("delete/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            Console.WriteLine($"[MessagingController] Deleting message {messageId}");

            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // First, get the message to verify ownership
                var messageResponse = await client
                    .From<DirectMessage>()
                    .Select("*")
                    .Filter("id", Operator.Equals, messageId)
                    .Get();

                var message = messageResponse.Models.FirstOrDefault();
                if (message == null)
                {
                    return NotFound(new { error = "Message not found" });
                }

                // Get current user ID from query parameter (in a real app, this would come from authentication)
                var currentUserId = Request.Query["userId"].FirstOrDefault();
                if (string.IsNullOrEmpty(currentUserId) || !int.TryParse(currentUserId, out int userId))
                {
                    return BadRequest(new { error = "userId parameter is required" });
                }

                // Only allow the sender to delete their own messages
                if (message.SenderId != userId)
                {
                    return Forbid("You can only delete your own messages");
                }

                // Delete the message
                await client
                    .From<DirectMessage>()
                    .Filter("id", Operator.Equals, messageId)
                    .Delete();

                Console.WriteLine($"✅ [MessagingController] Message {messageId} deleted successfully");
                return Ok(new { message = "Message deleted successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [MessagingController] Error deleting message: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }

    public class SendMessageRequest
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string MessageText { get; set; }
    }

    public class ConversationPreview
    {
        public int UserId { get; set; }
        public string LastMessage { get; set; }
        public DateTime? LastMessageTime { get; set; }
        public bool IsUnread { get; set; }
    }

    public class MessageDto
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string MessageText { get; set; }
        public DateTime? SentAt { get; set; }
        public bool IsRead { get; set; }
    }
}

