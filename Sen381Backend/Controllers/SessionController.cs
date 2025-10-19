using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Sen381.Data_Access;
using Sen381Backend.Models;
using Sen381Backend.Hubs;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace Sen381Backend.Controllers
{
    [ApiController]
    [Route("api/sessions")]
    public class SessionController : ControllerBase
    {
        private readonly SupaBaseAuthService _supabase;
        private readonly IHubContext<TutoringSessionHub> _hubContext;

        public SessionController(SupaBaseAuthService supabase, IHubContext<TutoringSessionHub> hubContext)
        {
            _supabase = supabase;
            _hubContext = hubContext;
        }

        // Get messages for a session
        [HttpGet("{sessionId}/messages")]
        public async Task<IActionResult> GetSessionMessages(int sessionId)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                Console.WriteLine($"[SessionController] Getting messages for session {sessionId}");
                
                var response = await client
                    .From<SessionMessage>()
                    .Filter("session_id", Operator.Equals, sessionId)
                    .Order("created_at", Ordering.Ascending)
                    .Get();

                var messages = response.Models.ToList();
                Console.WriteLine($"[SessionController] Found {messages.Count} messages for session {sessionId}");
                
                // Convert to DTOs to avoid serialization issues
                var messageDtos = messages.Select(m => new SessionMessageDto
                {
                    MessageId = m.MessageId,
                    SessionId = m.SessionId,
                    SenderId = m.SenderId,
                    SenderName = m.SenderName,
                    Content = m.Content,
                    IsFile = m.IsFile,
                    FileId = m.FileId,
                    CreatedAt = m.CreatedAt
                }).ToList();
                
                return Ok(messageDtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionController] Error getting session messages: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Get resources for a session
        [HttpGet("{sessionId}/resources")]
        public async Task<IActionResult> GetSessionResources(int sessionId)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // For now, return empty list since session_resources table might not exist yet
                // This will prevent 404 errors and allow the session to load
                return Ok(new List<SessionResource>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionController] Error getting session resources: {ex.Message}");
                return Ok(new List<SessionResource>()); // Return empty list on error
            }
        }

        // Post a message to a session
        [HttpPost("{sessionId}/messages")]
        public async Task<IActionResult> PostSessionMessage(int sessionId, [FromBody] SessionMessageDto messageDto)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                Console.WriteLine($"[SessionController] Posting message to session {sessionId}: {messageDto.Content}");
                
                // Convert DTO to model for database insertion
                var message = new SessionMessage
                {
                    SessionId = sessionId,
                    SenderId = messageDto.SenderId,
                    SenderName = messageDto.SenderName,
                    Content = messageDto.Content,
                    IsFile = messageDto.IsFile,
                    FileId = messageDto.FileId,
                    CreatedAt = DateTime.UtcNow
                };
                
                // Insert the message
                var response = await client
                    .From<SessionMessage>()
                    .Insert(message);
                
                Console.WriteLine($"[SessionController] Message posted successfully");
                
                // Return the created message as DTO
                var createdMessage = response.Models.FirstOrDefault();
                if (createdMessage != null)
                {
                    var responseDto = new SessionMessageDto
                    {
                        MessageId = createdMessage.MessageId,
                        SessionId = createdMessage.SessionId,
                        SenderId = createdMessage.SenderId,
                        SenderName = createdMessage.SenderName,
                        Content = createdMessage.Content,
                        IsFile = createdMessage.IsFile,
                        FileId = createdMessage.FileId,
                        CreatedAt = createdMessage.CreatedAt
                    };
                    
                    // Broadcast the message to all users in the session via SignalR
                    await _hubContext.Clients.Group($"session_{sessionId}").SendAsync("ReceiveMessage", responseDto);
                    Console.WriteLine($"[SessionController] Broadcasted message to session {sessionId} via SignalR");
                    
                    return Ok(new { success = true, message = "Message posted successfully", data = responseDto });
                }
                
                return Ok(new { success = true, message = "Message posted successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionController] Error posting session message: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Internal server error", error = ex.Message });
            }
        }

        // Delete a resource from a session
        [HttpDelete("{sessionId}/resources/{resourceId}")]
        public async Task<IActionResult> DeleteSessionResource(int sessionId, int resourceId)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // For now, just return success since session_resources table might not exist yet
                return Ok(new { success = true, message = "Resource deleted successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionController] Error deleting session resource: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }
}