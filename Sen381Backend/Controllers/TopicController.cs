using Microsoft.AspNetCore.Mvc;
using Sen381.Business.Models;
using Sen381.Business.Services;
using Sen381.Data_Access;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sen381Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TopicController : ControllerBase
    {
        private readonly TopicService _topicService;

        public TopicController(SupaBaseAuthService supabase)
        {
            _topicService = new TopicService(supabase);
        }

        /// <summary>
        /// Gets all topics for a specific subject
        /// </summary>
        [HttpGet("by-subject/{subjectId}")]
        public async Task<IActionResult> GetTopicsBySubject(int subjectId)
        {
            try
            {
                var topics = await _topicService.GetTopicsBySubjectAsync(subjectId);
                var topicDtos = topics.Select(t => new TopicDto
                {
                    TopicId = t.TopicId,
                    SubjectId = t.SubjectId,
                    Title = t.Title,
                    Description = t.Description,
                    OrderNumber = t.OrderNumber,
                    IsActive = t.IsActive,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                }).ToList();
                
                return Ok(topicDtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TopicController] Error getting topics for subject {subjectId}: {ex.Message}");
                return StatusCode(500, new { error = "Failed to retrieve topics" });
            }
        }

        /// <summary>
        /// Gets a topic by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTopic(int id)
        {
            try
            {
                var topic = await _topicService.GetTopicByIdAsync(id);
                if (topic == null)
                    return NotFound(new { error = "Topic not found" });

                var topicDto = new TopicDto
                {
                    TopicId = topic.TopicId,
                    SubjectId = topic.SubjectId,
                    Title = topic.Title,
                    Description = topic.Description,
                    OrderNumber = topic.OrderNumber,
                    IsActive = topic.IsActive,
                    CreatedAt = topic.CreatedAt,
                    UpdatedAt = topic.UpdatedAt
                };

                return Ok(topicDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TopicController] Error getting topic {id}: {ex.Message}");
                return StatusCode(500, new { error = "Failed to retrieve topic" });
            }
        }

        /// <summary>
        /// Creates a new topic
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateTopic([FromBody] CreateTopicDto dto)
        {
            try
            {
                Console.WriteLine($"[TopicController] Received CreateTopic request: SubjectId={dto?.SubjectId}, Title='{dto?.Title}'");
                
                if (dto == null)
                    return BadRequest(new { error = "Invalid topic data" });
                
                if (string.IsNullOrWhiteSpace(dto.Title))
                    return BadRequest(new { error = "Topic title is required" });

                if (dto.SubjectId <= 0)
                    return BadRequest(new { error = "Valid subject ID is required" });

                // Check if topic title already exists within the subject
                if (await _topicService.TopicTitleExistsAsync(dto.SubjectId, dto.Title))
                    return BadRequest(new { error = "Topic title already exists in this subject" });

                var topic = new TopicDb
                {
                    SubjectId = dto.SubjectId,
                    Title = dto.Title.Trim(),
                    Description = dto.Description?.Trim(),
                    IsActive = dto.IsActive
                };

                var createdTopic = await _topicService.CreateTopicAsync(topic);
                
                var topicDto = new TopicDto
                {
                    TopicId = createdTopic.TopicId,
                    SubjectId = createdTopic.SubjectId,
                    Title = createdTopic.Title,
                    Description = createdTopic.Description,
                    OrderNumber = createdTopic.OrderNumber,
                    IsActive = createdTopic.IsActive,
                    CreatedAt = createdTopic.CreatedAt,
                    UpdatedAt = createdTopic.UpdatedAt
                };
                
                return CreatedAtAction(nameof(GetTopic), new { id = createdTopic.TopicId }, topicDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TopicController] Error creating topic: {ex.Message}");
                return StatusCode(500, new { error = "Failed to create topic" });
            }
        }

        /// <summary>
        /// Updates an existing topic
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTopic(int id, [FromBody] UpdateTopicDto dto)
        {
            try
            {
                var existingTopic = await _topicService.GetTopicByIdAsync(id);
                if (existingTopic == null)
                    return NotFound(new { error = "Topic not found" });

                if (string.IsNullOrWhiteSpace(dto.Title))
                    return BadRequest(new { error = "Topic title is required" });

                // Check if topic title already exists within the subject (excluding current topic)
                if (await _topicService.TopicTitleExistsAsync(existingTopic.SubjectId, dto.Title, id))
                    return BadRequest(new { error = "Topic title already exists in this subject" });

                existingTopic.Title = dto.Title.Trim();
                existingTopic.Description = dto.Description?.Trim();
                existingTopic.IsActive = dto.IsActive;

                var updatedTopic = await _topicService.UpdateTopicAsync(existingTopic);
                
                var topicDto = new TopicDto
                {
                    TopicId = updatedTopic.TopicId,
                    SubjectId = updatedTopic.SubjectId,
                    Title = updatedTopic.Title,
                    Description = updatedTopic.Description,
                    OrderNumber = updatedTopic.OrderNumber,
                    IsActive = updatedTopic.IsActive,
                    CreatedAt = updatedTopic.CreatedAt,
                    UpdatedAt = updatedTopic.UpdatedAt
                };
                
                return Ok(topicDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TopicController] Error updating topic {id}: {ex.Message}");
                return StatusCode(500, new { error = "Failed to update topic" });
            }
        }

        /// <summary>
        /// Deletes a topic
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTopic(int id)
        {
            try
            {
                var existingTopic = await _topicService.GetTopicByIdAsync(id);
                if (existingTopic == null)
                    return NotFound(new { error = "Topic not found" });

                await _topicService.DeleteTopicAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TopicController] Error deleting topic {id}: {ex.Message}");
                return StatusCode(500, new { error = "Failed to delete topic" });
            }
        }

        /// <summary>
        /// Updates the order of topics within a subject
        /// </summary>
        [HttpPut("reorder/{subjectId}")]
        public async Task<IActionResult> ReorderTopics(int subjectId, [FromBody] ReorderTopicsDto dto)
        {
            try
            {
                if (dto.TopicIds == null || dto.TopicIds.Count == 0)
                    return BadRequest(new { error = "Topic IDs are required" });

                await _topicService.UpdateTopicOrderAsync(subjectId, dto.TopicIds);
                return Ok(new { message = "Topics reordered successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TopicController] Error reordering topics for subject {subjectId}: {ex.Message}");
                return StatusCode(500, new { error = "Failed to reorder topics" });
            }
        }
    }

    public class CreateTopicDto
    {
        public int SubjectId { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateTopicDto
    {
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class ReorderTopicsDto
    {
        public List<int> TopicIds { get; set; } = new List<int>();
    }

    public class TopicDto
    {
        public int TopicId { get; set; }
        public int SubjectId { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public int OrderNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
