using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Sen381.Data_Access;
using Sen381Backend.Models;
using Sen381Backend.Hubs;
using Sen381.Business.Models;
using System;
using System.IO;
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
                
                // Get unique sender IDs to fetch their names
                var senderIds = messages.Select(m => m.SenderId).Distinct().ToList();
                var users = new Dictionary<int, string>();
                
                foreach (var senderId in senderIds)
                {
                    try
                    {
                        var userResponse = await client
                            .From<User>()
                            .Select("first_name, last_name")
                            .Filter("user_id", Operator.Equals, senderId)
                            .Get();
                        
                        var user = userResponse.Models.FirstOrDefault();
                        if (user != null)
                        {
                            var fullName = $"{user.FirstName} {user.LastName}".Trim();
                            users[senderId] = string.IsNullOrEmpty(fullName) ? "Unknown User" : fullName;
                        }
                        else
                        {
                            users[senderId] = "Unknown User";
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SessionController] Error fetching user {senderId}: {ex.Message}");
                        users[senderId] = "Unknown User";
                    }
                }
                
                // Convert to DTOs with proper sender names
                var messageDtos = messages.Select(m => new SessionMessageDto
                {
                    MessageId = m.MessageId,
                    SessionId = m.SessionId,
                    SenderId = m.SenderId,
                    SenderName = users.ContainsKey(m.SenderId) ? users[m.SenderId] : "Unknown User",
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

                Console.WriteLine($"[SessionController] Getting resources for session {sessionId}");

                // Try to get resources from session_resources table
                try
                {
                    var response = await client
                        .From<SessionResource>()
                        .Filter("session_id", Operator.Equals, sessionId)
                        .Order("uploaded_at", Ordering.Descending)
                        .Get();

                    var resources = response.Models.ToList();
                    Console.WriteLine($"[SessionController] Found {resources.Count} resources for session {sessionId}");
                    
                    // Convert to clean DTOs for response
                    var resourceDtos = resources.Select(r => new SessionResourceResponseDto
                    {
                        FileId = r.FileId,
                        SessionId = r.SessionId,
                        UploaderId = r.UploaderId,
                        FileName = r.FileName,
                        FileSizeBytes = r.FileSizeBytes,
                        FilePath = r.FilePath,
                        UploadedAt = r.UploadedAt
                    }).ToList();
                    
                    return Ok(resourceDtos);
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine($"[SessionController] Database error getting resources: {dbEx.Message}");
                    Console.WriteLine($"[SessionController] This might be because the session_resources table doesn't exist yet");
                    
                    // Return empty list if table doesn't exist yet
                    return Ok(new List<SessionResource>());
                }
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
                
                // Fetch the sender's name from the database
                string senderName = "Unknown User";
                try
                {
                    var userResponse = await client
                        .From<User>()
                        .Select("first_name, last_name")
                        .Filter("user_id", Operator.Equals, messageDto.SenderId)
                        .Get();
                    
                    var user = userResponse.Models.FirstOrDefault();
                    if (user != null)
                    {
                        var fullName = $"{user.FirstName} {user.LastName}".Trim();
                        senderName = string.IsNullOrEmpty(fullName) ? "Unknown User" : fullName;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SessionController] Error fetching sender name for user {messageDto.SenderId}: {ex.Message}");
                }
                
                // Convert DTO to model for database insertion
                var message = new SessionMessage
                {
                    SessionId = sessionId,
                    SenderId = messageDto.SenderId,
                    SenderName = senderName,
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
                        SenderName = senderName, // Use the fetched sender name
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

        // Upload a file resource to a session
        [HttpPost("{sessionId}/resources")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadSessionResource(int sessionId, [FromForm] FileUploadDto uploadDto)
        {
            try
            {
                if (uploadDto?.File == null || uploadDto.File.Length == 0)
                    return BadRequest(new { success = false, message = "No file provided" });

                // Validate file type and size
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".jpg", ".jpeg", ".png", ".gif", ".txt" };
                var fileExtension = Path.GetExtension(uploadDto.File.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                    return BadRequest(new { success = false, message = "File type not allowed. Supported types: PDF, DOC, DOCX, PPT, PPTX, JPG, PNG, GIF, TXT" });

                if (uploadDto.File.Length > 10 * 1024 * 1024) // 10MB limit
                    return BadRequest(new { success = false, message = "File size too large. Maximum size is 10MB" });

                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                Console.WriteLine($"[SessionController] Uploading file '{uploadDto.File.FileName}' to session {sessionId}");

                // Upload file to Supabase storage
                using var ms = new MemoryStream();
                await uploadDto.File.CopyToAsync(ms);
                var fileBytes = ms.ToArray();

                var bucket = client.Storage.From("User_Uploads");
                var fileName = $"{Guid.NewGuid()}_{uploadDto.File.FileName}";

                await bucket.Upload(fileBytes, fileName);
                var signedUrl = await bucket.CreateSignedUrl(fileName, 86400); // 24 hours

                // Save resource metadata to database
                var resource = new SessionResource
                {
                    SessionId = sessionId,
                    UploaderId = uploadDto.UploaderId ?? "unknown",
                    FileName = uploadDto.File.FileName,
                    FileSizeBytes = uploadDto.File.Length,
                    FilePath = signedUrl,
                    UploadedAt = DateTime.UtcNow
                };

                try
                {
                    Console.WriteLine($"[SessionController] Attempting to save resource metadata to database...");
                    var response = await client
                        .From<SessionResource>()
                        .Insert(resource);

                    var createdResource = response.Models.FirstOrDefault();
                    if (createdResource != null)
                    {
                        Console.WriteLine($"[SessionController] File uploaded and saved successfully: {uploadDto.File.FileName}, FileId: {createdResource.FileId}");
                        
                        // Convert to clean DTO for response
                        var resourceDto = new SessionResourceResponseDto
                        {
                            FileId = createdResource.FileId,
                            SessionId = createdResource.SessionId,
                            UploaderId = createdResource.UploaderId,
                            FileName = createdResource.FileName,
                            FileSizeBytes = createdResource.FileSizeBytes,
                            FilePath = createdResource.FilePath,
                            UploadedAt = createdResource.UploadedAt
                        };
                        
                        // Broadcast the new resource to all users in the session
                        try
                        {
                            await _hubContext.Clients.Group($"session_{sessionId}").SendAsync("ReceiveResource", resourceDto);
                            Console.WriteLine($"[SessionController] Resource broadcast sent to session {sessionId}");
                        }
                        catch (Exception hubEx)
                        {
                            Console.WriteLine($"[SessionController] Error broadcasting resource: {hubEx.Message}");
                        }

                        return Ok(new { 
                            success = true, 
                            message = "File uploaded successfully",
                            resource = resourceDto
                        });
                    }
                    else
                    {
                        Console.WriteLine($"[SessionController] Warning: No resource returned from database insert");
                        return Ok(new { 
                            success = true, 
                            message = "File uploaded to storage but no metadata returned",
                            fileName = uploadDto.File.FileName,
                            filePath = signedUrl
                        });
                    }
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine($"[SessionController] Database error saving resource: {dbEx.Message}");
                    Console.WriteLine($"[SessionController] Stack trace: {dbEx.StackTrace}");
                    
                    // Still return success since file was uploaded to storage
                    return Ok(new { 
                        success = true, 
                        message = "File uploaded to storage but metadata not saved - database error",
                        fileName = uploadDto.File.FileName,
                        filePath = signedUrl,
                        error = dbEx.Message
                    });
                }

                return StatusCode(500, new { success = false, message = "Failed to save resource metadata" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionController] Error uploading session resource: {ex.Message}");
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

                Console.WriteLine($"[SessionController] Deleting resource {resourceId} from session {sessionId}");

                try
                {
                    // First get the resource to get the file path
                    var resourceResponse = await client
                        .From<SessionResource>()
                        .Filter("file_id", Operator.Equals, resourceId)
                        .Filter("session_id", Operator.Equals, sessionId)
                        .Get();

                    var resource = resourceResponse.Models.FirstOrDefault();
                    if (resource == null)
                    {
                        return NotFound(new { success = false, message = "Resource not found" });
                    }

                    // Delete from database
                    await client
                        .From<SessionResource>()
                        .Filter("file_id", Operator.Equals, resourceId)
                        .Delete();

                    Console.WriteLine($"[SessionController] Resource {resourceId} deleted successfully");
                    
                    // Broadcast the deletion to all users in the session
                    await _hubContext.Clients.Group($"session_{sessionId}").SendAsync("ResourceDeleted", resourceId);

                    return Ok(new { success = true, message = "Resource deleted successfully" });
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine($"[SessionController] Database error deleting resource: {dbEx.Message}");
                    return StatusCode(500, new { success = false, message = "Failed to delete resource from database" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SessionController] Error deleting session resource: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }
    }
}