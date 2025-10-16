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
    public class SubjectController : ControllerBase
    {
        private readonly SubjectService _subjectService;

        public SubjectController(SupaBaseAuthService supabase)
        {
            _subjectService = new SubjectService(supabase);
        }

        /// <summary>
        /// Gets all subjects
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllSubjects()
        {
            try
            {
                var subjects = await _subjectService.GetAllSubjectsAsync();
                var subjectDtos = subjects.Select(s => new SubjectDto
                {
                    SubjectId = s.SubjectId,
                    SubjectCode = s.SubjectCode,
                    Name = s.Name,
                    Description = s.Description,
                    Year = s.Year,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                }).ToList();
                
                return Ok(subjectDtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SubjectController] Error getting subjects: {ex.Message}");
                return StatusCode(500, new { error = "Failed to retrieve subjects" });
            }
        }

        /// <summary>
        /// Gets a subject by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubject(int id)
        {
            try
            {
                var subject = await _subjectService.GetSubjectByIdAsync(id);
                if (subject == null)
                    return NotFound(new { error = "Subject not found" });

                var subjectDto = new SubjectDto
                {
                    SubjectId = subject.SubjectId,
                    SubjectCode = subject.SubjectCode,
                    Name = subject.Name,
                    Description = subject.Description,
                    Year = subject.Year,
                    IsActive = subject.IsActive,
                    CreatedAt = subject.CreatedAt,
                    UpdatedAt = subject.UpdatedAt
                };

                return Ok(subjectDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SubjectController] Error getting subject {id}: {ex.Message}");
                return StatusCode(500, new { error = "Failed to retrieve subject" });
            }
        }

        /// <summary>
        /// Creates a new subject
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.SubjectCode))
                    return BadRequest(new { error = "Subject code is required" });

                if (string.IsNullOrWhiteSpace(dto.Name))
                    return BadRequest(new { error = "Subject name is required" });

                if (dto.Year < 1 || dto.Year > 5)
                    return BadRequest(new { error = "Year must be between 1 and 5" });

                // Check if subject code already exists
                if (await _subjectService.SubjectCodeExistsAsync(dto.SubjectCode))
                    return BadRequest(new { error = "Subject code already exists" });

                var subject = new SubjectDb
                {
                    SubjectCode = dto.SubjectCode.Trim(),
                    Name = dto.Name.Trim(),
                    Description = dto.Description?.Trim(),
                    Year = dto.Year,
                    IsActive = dto.IsActive
                };

                var createdSubject = await _subjectService.CreateSubjectAsync(subject);
                
                var subjectDto = new SubjectDto
                {
                    SubjectId = createdSubject.SubjectId,
                    SubjectCode = createdSubject.SubjectCode,
                    Name = createdSubject.Name,
                    Description = createdSubject.Description,
                    Year = createdSubject.Year,
                    IsActive = createdSubject.IsActive,
                    CreatedAt = createdSubject.CreatedAt,
                    UpdatedAt = createdSubject.UpdatedAt
                };
                
                return CreatedAtAction(nameof(GetSubject), new { id = createdSubject.SubjectId }, subjectDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SubjectController] Error creating subject: {ex.Message}");
                return StatusCode(500, new { error = "Failed to create subject" });
            }
        }

        /// <summary>
        /// Updates an existing subject
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubject(int id, [FromBody] UpdateSubjectDto dto)
        {
            try
            {
                var existingSubject = await _subjectService.GetSubjectByIdAsync(id);
                if (existingSubject == null)
                    return NotFound(new { error = "Subject not found" });

                if (string.IsNullOrWhiteSpace(dto.SubjectCode))
                    return BadRequest(new { error = "Subject code is required" });

                if (string.IsNullOrWhiteSpace(dto.Name))
                    return BadRequest(new { error = "Subject name is required" });

                if (dto.Year < 1 || dto.Year > 5)
                    return BadRequest(new { error = "Year must be between 1 and 5" });

                // Check if subject code already exists (excluding current subject)
                if (await _subjectService.SubjectCodeExistsAsync(dto.SubjectCode, id))
                    return BadRequest(new { error = "Subject code already exists" });

                existingSubject.SubjectCode = dto.SubjectCode.Trim();
                existingSubject.Name = dto.Name.Trim();
                existingSubject.Description = dto.Description?.Trim();
                existingSubject.Year = dto.Year;
                existingSubject.IsActive = dto.IsActive;

                var updatedSubject = await _subjectService.UpdateSubjectAsync(existingSubject);
                
                var subjectDto = new SubjectDto
                {
                    SubjectId = updatedSubject.SubjectId,
                    SubjectCode = updatedSubject.SubjectCode,
                    Name = updatedSubject.Name,
                    Description = updatedSubject.Description,
                    Year = updatedSubject.Year,
                    IsActive = updatedSubject.IsActive,
                    CreatedAt = updatedSubject.CreatedAt,
                    UpdatedAt = updatedSubject.UpdatedAt
                };
                
                return Ok(subjectDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SubjectController] Error updating subject {id}: {ex.Message}");
                return StatusCode(500, new { error = "Failed to update subject" });
            }
        }

        /// <summary>
        /// Deletes a subject
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubject(int id)
        {
            try
            {
                var existingSubject = await _subjectService.GetSubjectByIdAsync(id);
                if (existingSubject == null)
                    return NotFound(new { error = "Subject not found" });

                await _subjectService.DeleteSubjectAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SubjectController] Error deleting subject {id}: {ex.Message}");
                return StatusCode(500, new { error = "Failed to delete subject" });
            }
        }

        /// <summary>
        /// Toggles the active status of a subject
        /// </summary>
        [HttpPatch("{id}/toggle-active")]
        public async Task<IActionResult> ToggleActive(int id, [FromBody] ToggleActiveDto dto)
        {
            try
            {
                var existingSubject = await _subjectService.GetSubjectByIdAsync(id);
                if (existingSubject == null)
                    return NotFound(new { error = "Subject not found" });

                existingSubject.IsActive = dto.IsActive;
                var updatedSubject = await _subjectService.UpdateSubjectAsync(existingSubject);

                var subjectDto = new SubjectDto
                {
                    SubjectId = updatedSubject.SubjectId,
                    SubjectCode = updatedSubject.SubjectCode,
                    Name = updatedSubject.Name,
                    Description = updatedSubject.Description,
                    Year = updatedSubject.Year,
                    IsActive = updatedSubject.IsActive,
                    CreatedAt = updatedSubject.CreatedAt,
                    UpdatedAt = updatedSubject.UpdatedAt
                };

                return Ok(subjectDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SubjectController] Error toggling active status for subject {id}: {ex.Message}");
                return StatusCode(500, new { error = "Failed to update subject status" });
            }
        }
    }

    public class CreateSubjectDto
    {
        public string SubjectCode { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public int Year { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateSubjectDto
    {
        public string SubjectCode { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public int Year { get; set; }
        public bool IsActive { get; set; }
    }

    public class ToggleActiveDto
    {
        public bool IsActive { get; set; }
    }

    public class SubjectDto
    {
        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public int Year { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
