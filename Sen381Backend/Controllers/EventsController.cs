using Microsoft.AspNetCore.Mvc;
using Sen381.Data_Access;          // ✅ for SupaBaseAuthService
using Sen381Backend.Models;
using Supabase;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sen381Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly SupaBaseAuthService _supabase;

        public EventsController(SupaBaseAuthService supabase)
        {
            _supabase = supabase;
        }

        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetForUser(int userId)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var resp = await client
                    .From<UserEvent>()
                    .Where(e => e.UserId == userId)
                    .Order("when_at", Supabase.Postgrest.Constants.Ordering.Ascending)
                    .Get();

                var result = resp.Models.Select(m => new CreateEventDto
                {
                    Id = m.Id,
                    UserId = m.UserId,
                    Title = m.Title,
                    WhenAt = m.WhenAt,
                    DurationMinutes = m.DurationMinutes
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] GetForUser: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        public class CreateEventDto
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public string Title { get; set; } = string.Empty;
            public DateTime WhenAt { get; set; }
            public int DurationMinutes { get; set; } = 60;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
        {
            if (dto.UserId <= 0 || string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { error = "Invalid event." });
            if (dto.WhenAt.Date < DateTime.Today)
                return BadRequest(new { error = "Cannot create events in the past." });

            await _supabase.InitializeAsync();
            var client = _supabase.Client;

            var model = new UserEvent
            {
                UserId = dto.UserId,
                Title = dto.Title,
                WhenAt = dto.WhenAt,
                DurationMinutes = dto.DurationMinutes,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                // Limit to 5 events per day
                var dayStart = new DateTime(dto.WhenAt.Year, dto.WhenAt.Month, dto.WhenAt.Day, 0, 0, 0, DateTimeKind.Unspecified);
                var dayEnd = dayStart.AddDays(1);

                var allUserEvents = await client
                    .From<UserEvent>()
                    .Where(e => e.UserId == dto.UserId)
                    .Get();

                var existing = allUserEvents.Models.Where(e => e.WhenAt >= dayStart && e.WhenAt < dayEnd);
                if (existing.Count() >= 5)
                    return BadRequest(new { error = "You can only have up to 5 events per day." });

                var inserted = await client.From<UserEvent>().Insert(model);
                var saved = inserted.Models.FirstOrDefault() ?? model;

                var dtoOut = new CreateEventDto
                {
                    Id = saved.Id,
                    UserId = saved.UserId,
                    Title = saved.Title,
                    WhenAt = saved.WhenAt,
                    DurationMinutes = saved.DurationMinutes
                };

                return Ok(dtoOut);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Create: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateEventDto dto)
        {
            try
            {
                if (dto.WhenAt.Date < DateTime.Today)
                    return BadRequest(new { error = "Cannot move events to the past." });

                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // Limit to 5 events per day (exclude current event)
                var dayStart = new DateTime(dto.WhenAt.Year, dto.WhenAt.Month, dto.WhenAt.Day, 0, 0, 0, DateTimeKind.Unspecified);
                var dayEnd = dayStart.AddDays(1);

                var allUserEvents = await client
                    .From<UserEvent>()
                    .Where(e => e.UserId == dto.UserId)
                    .Get();

                var clash = allUserEvents.Models.Where(e => e.WhenAt >= dayStart && e.WhenAt < dayEnd && e.Id != id);
                if (clash.Count() >= 5)
                    return BadRequest(new { error = "You can only have up to 5 events per day." });

                var ev = new UserEvent
                {
                    Id = id,
                    Title = dto.Title,
                    WhenAt = dto.WhenAt,
                    DurationMinutes = dto.DurationMinutes
                };

                var updated = await client.From<UserEvent>().Update(ev);
                var saved = updated.Models.FirstOrDefault() ?? ev;

                return Ok(new CreateEventDto
                {
                    Id = saved.Id,
                    UserId = saved.UserId,
                    Title = saved.Title,
                    WhenAt = saved.WhenAt,
                    DurationMinutes = saved.DurationMinutes
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Update: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                await client.From<UserEvent>().Where(e => e.Id == id).Delete();
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Delete: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
