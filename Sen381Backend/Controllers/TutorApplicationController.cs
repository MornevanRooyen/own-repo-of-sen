using Microsoft.AspNetCore.Mvc;
using Sen381Backend.Models;
using Sen381.Data_Access;
using Sen381.Business.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace Sen381Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TutorApplicationController : ControllerBase
    {
        private readonly SupaBaseAuthService _supabase;

        public TutorApplicationController(SupaBaseAuthService supabase)
        {
            _supabase = supabase;
        }

        // Submit a new tutor application
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitApplication([FromBody] TutorApplicationInput input)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // First, get user details
                var userResponse = await client
                    .From<User>()
                    .Select("first_name, last_name, email, profile_picture_path")
                    .Filter("user_id", Operator.Equals, input.UserId)
                    .Get();

                var user = userResponse.Models.FirstOrDefault();
                if (user == null)
                    return NotFound(new TutorApplicationResponse { Success = false, Message = "User not found" });

                // Check if user already has a pending application
                var existingResponse = await client
                    .From<TutorApplication>()
                    .Select("application_id")
                    .Filter("user_id", Operator.Equals, input.UserId)
                    .Filter("status", Operator.Equals, "pending")
                    .Get();

                if (existingResponse.Models.Any())
                    return BadRequest(new TutorApplicationResponse { Success = false, Message = "You already have a pending tutor application" });

                // Create new application
                var application = new TutorApplication
                {
                    UserId = input.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNum = input.PhoneNum,
                    StudentNo = input.StudentNo,
                    Major = input.Major,
                    YearOfStudy = input.YearOfStudy,
                    MinRequiredGrade = input.MinRequiredGrade,
                    ProfilePicturePath = user.ProfilePicturePath,
                    TranscriptPath = input.TranscriptPath,
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow
                };

                var insertResponse = await client
                    .From<TutorApplication>()
                    .Insert(application);

                var insertedApp = insertResponse.Models.FirstOrDefault();
                if (insertedApp == null)
                    return StatusCode(500, new TutorApplicationResponse { Success = false, Message = "Failed to submit application" });

                return Ok(new TutorApplicationResponse 
                { 
                    Success = true, 
                    Message = "Tutor application submitted successfully", 
                    ApplicationId = insertedApp.ApplicationId 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TutorApplicationController] Error (SubmitApplication): {ex.Message}");
                return StatusCode(500, new TutorApplicationResponse { Success = false, Message = "Internal server error" });
            }
        }

        // Get all pending tutor applications (for admin)
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingApplications()
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var response = await client
                    .From<TutorApplication>()
                    .Select("*")
                    .Filter("status", Operator.Equals, "pending")
                    .Order("created_at", Ordering.Descending)
                    .Get();

                return Ok(response.Models);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TutorApplicationController] Error (GetPendingApplications): {ex.Message}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // Approve a tutor application
        [HttpPost("{applicationId}/approve")]
        public async Task<IActionResult> ApproveApplication(int applicationId, [FromBody] ApproveApplicationRequest request)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // Get the application
                var appResponse = await client
                    .From<TutorApplication>()
                    .Select("*")
                    .Filter("application_id", Operator.Equals, applicationId)
                    .Get();

                var application = appResponse.Models.FirstOrDefault();
                if (application == null)
                    return NotFound(new { error = "Application not found" });

                if (application.Status != "pending")
                    return BadRequest(new { error = "Application has already been reviewed" });

                // Update application status
                var updateResponse = await client
                    .From<TutorApplication>()
                    .Set(x => x.Status, "approved")
                    .Set(x => x.ReviewedAt, DateTime.UtcNow)
                    .Set(x => x.ReviewedBy, request.AdminUserId)
                    .Set(x => x.ReviewNotes, request.Notes)
                    .Filter("application_id", Operator.Equals, applicationId)
                    .Update();

                // Update user role to tutor
                await client
                    .From<User>()
                    .Set(x => x.RoleString, "tutor")
                    .Filter("user_id", Operator.Equals, application.UserId)
                    .Update();

                return Ok(new { success = true, message = "Application approved successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TutorApplicationController] Error (ApproveApplication): {ex.Message}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // Decline a tutor application
        [HttpPost("{applicationId}/decline")]
        public async Task<IActionResult> DeclineApplication(int applicationId, [FromBody] DeclineApplicationRequest request)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // Get the application
                var appResponse = await client
                    .From<TutorApplication>()
                    .Select("*")
                    .Filter("application_id", Operator.Equals, applicationId)
                    .Get();

                var application = appResponse.Models.FirstOrDefault();
                if (application == null)
                    return NotFound(new { error = "Application not found" });

                if (application.Status != "pending")
                    return BadRequest(new { error = "Application has already been reviewed" });

                // Update application status
                var updateResponse = await client
                    .From<TutorApplication>()
                    .Set(x => x.Status, "declined")
                    .Set(x => x.ReviewedAt, DateTime.UtcNow)
                    .Set(x => x.ReviewedBy, request.AdminUserId)
                    .Set(x => x.ReviewNotes, request.Notes)
                    .Filter("application_id", Operator.Equals, applicationId)
                    .Update();

                return Ok(new { success = true, message = "Application declined" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TutorApplicationController] Error (DeclineApplication): {ex.Message}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // Get application status for a user
        [HttpGet("user/{userId}/status")]
        public async Task<IActionResult> GetUserApplicationStatus(int userId)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var response = await client
                    .From<TutorApplication>()
                    .Select("*")
                    .Filter("user_id", Operator.Equals, userId)
                    .Order("created_at", Ordering.Descending)
                    .Get();

                var application = response.Models.FirstOrDefault();
                if (application == null)
                    return Ok(new { hasApplication = false });

                return Ok(new 
                { 
                    hasApplication = true, 
                    status = application.Status,
                    createdAt = application.CreatedAt,
                    reviewedAt = application.ReviewedAt,
                    reviewNotes = application.ReviewNotes
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TutorApplicationController] Error (GetUserApplicationStatus): {ex.Message}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }

    public class ApproveApplicationRequest
    {
        public int AdminUserId { get; set; }
        public string? Notes { get; set; }
    }

    public class DeclineApplicationRequest
    {
        public int AdminUserId { get; set; }
        public string? Notes { get; set; }
    }
}
