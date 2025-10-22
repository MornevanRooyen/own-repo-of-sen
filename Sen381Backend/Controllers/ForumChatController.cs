using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Sen381Backend.Models;
using Sen381Backend.Hubs;
using Sen381.Data_Access;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace Sen381Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForumChatController : ControllerBase
    {
        private readonly SupaBaseAuthService _supabase;
        private readonly IHubContext<ForumChatHub> _hubContext;

        public ForumChatController(SupaBaseAuthService supabase, IHubContext<ForumChatHub> hubContext)
        {
            _supabase = supabase;
            _hubContext = hubContext;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ForumMessage message)
        {
            await _supabase.InitializeAsync();
            var client = _supabase.Client;

            message.CreatedAt = DateTime.UtcNow;
            var response = await client.From<ForumMessage>().Insert(message);

            var inserted = response.Models.FirstOrDefault();
            if (inserted == null)
                return BadRequest("Failed to insert message.");

            // Convert to DTO
            var dto = new ForumMessageDto
            {
                MessageId = inserted.MessageId,
                ForumId = inserted.ForumId,
                SenderId = inserted.SenderId,
                SenderName = inserted.SenderName,
                Content = inserted.Content,
                IsFile = inserted.IsFile,
                FileId = inserted.FileId,
                CreatedAt = inserted.CreatedAt
            };

            // Broadcast the DTO instead of the raw model
            await _hubContext.Clients.Group($"forum_{message.ForumId}")
                .SendAsync("ReceiveForumMessage", dto);

            return Ok(dto);
        }


        [HttpGet("{forumId}")]
        public async Task<IActionResult> GetForumMessages(int forumId)
        {
            await _supabase.InitializeAsync();
            var client = _supabase.Client;

            var response = await client
                .From<ForumMessage>()
                .Select("*")
                .Filter("forum_id", Operator.Equals, forumId)
                .Order("created_at", Ordering.Ascending)
                .Get();

            var dtoList = response.Models.Select(m => new ForumMessageDto
            {
                MessageId = m.MessageId,
                ForumId = m.ForumId,
                SenderId = m.SenderId,
                SenderName = m.SenderName,
                Content = m.Content,
                IsFile = m.IsFile,
                FileId = m.FileId,
                CreatedAt = m.CreatedAt
            }).ToList();

            return Ok(dtoList);

        }
    }
}
