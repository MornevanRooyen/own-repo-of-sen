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
    public class SubjectTutorController : ControllerBase
    {
        private readonly SupaBaseAuthService _supabase;

        public SubjectTutorController(SupaBaseAuthService supabase)
        {
            _supabase = supabase;
        }

        // Get all tutors for a specific subject
        [HttpGet("subject/{subjectId}")]
        public async Task<IActionResult> GetTutorsForSubject(int subjectId, [FromQuery] int? currentUserId = null)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // Get tutors for the specific subject
                var response = await client
                    .From<SubjectTutor>()
                    .Select("*")
                    .Filter("subject_id", Operator.Equals, subjectId)
                    .Get();

                Console.WriteLine($"[SubjectTutorController] Found {response.Models.Count} tutor records for subject {subjectId}");
                foreach (var st in response.Models)
                {
                    Console.WriteLine($"[SubjectTutorController] Tutor: UserId={st.UserId}, SubjectId={st.SubjectId}, IsActive={st.IsActive}");
                }

                // Debug: Let's also check what all tutor records exist for this user
                if (response.Models.Any())
                {
                    var userIds = response.Models.Select(t => t.UserId).Distinct();
                    foreach (var userId in userIds)
                    {
                        var allTutorRecords = await client
                            .From<SubjectTutor>()
                            .Select("*")
                            .Filter("user_id", Operator.Equals, userId)
                            .Get();
                        
                        Console.WriteLine($"[SubjectTutorController] User {userId} has {allTutorRecords.Models.Count} tutor records total:");
                        foreach (var record in allTutorRecords.Models)
                        {
                            Console.WriteLine($"[SubjectTutorController]   - SubjectId={record.SubjectId}, IsActive={record.IsActive}");
                        }
                    }
                }

                // Get user and subject details for each active tutor
                var tutors = new List<SubjectTutorDto>();
                foreach (var st in response.Models.Where(t => t.IsActive))
                {
                    // Get user details
                    var userResponse = await client
                        .From<User>()
                        .Select("first_name, last_name, email, profile_picture_path")
                        .Filter("user_id", Operator.Equals, st.UserId)
                        .Get();

                    var user = userResponse.Models.FirstOrDefault();

                    // Get subject details
                    var subjectResponse = await client
                        .From<Sen381Backend.Models.Subject>()
                        .Select("subject_code, name, year")
                        .Filter("subject_id", Operator.Equals, st.SubjectId)
                        .Get();

                    var subject = subjectResponse.Models.FirstOrDefault();

                    // Check if current user is following this tutor
                    bool isFollowing = false;
                    if (currentUserId.HasValue)
                    {
                        var followingResponse = await client
                            .From<UserFollow>()
                            .Select("id")
                            .Filter("follower_id", Operator.Equals, currentUserId.Value)
                            .Filter("following_id", Operator.Equals, st.UserId)
                            .Get();
                        
                        isFollowing = followingResponse.Models.Any();
                    }

                    tutors.Add(new SubjectTutorDto
                    {
                        SubjectTutorId = st.SubjectTutorId,
                        UserId = st.UserId,
                        SubjectId = st.SubjectId,
                        IsActive = st.IsActive,
                        ApprovedAt = st.ApprovedAt,
                        ApprovedBy = st.ApprovedBy,
                        CreatedAt = st.CreatedAt,
                        FirstName = user?.FirstName ?? "",
                        LastName = user?.LastName ?? "",
                        Email = user?.Email ?? "",
                        ProfilePicturePath = user?.ProfilePicturePath,
                        SubjectCode = subject?.SubjectCode ?? "",
                        SubjectName = subject?.Name ?? "",
                        SubjectYear = subject?.Year ?? 0,
                        Following = isFollowing
                    });
                }

                Console.WriteLine($"[SubjectTutorController] Returning {tutors.Count} active tutors for subject {subjectId}");
                return Ok(tutors);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SubjectTutorController] Error (GetTutorsForSubject): {ex.Message}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // Get all subjects a user can tutor
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetSubjectsForTutor(int userId)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var response = await client
                    .From<SubjectTutor>()
                    .Select("*")
                    .Filter("user_id", Operator.Equals, userId)
                    .Get();

                var subjects = new List<object>();
                foreach (var st in response.Models.Where(t => t.IsActive))
                {
                    // Get subject details
                    var subjectResponse = await client
                        .From<Sen381Backend.Models.Subject>()
                        .Select("subject_code, name, year")
                        .Filter("subject_id", Operator.Equals, st.SubjectId)
                        .Get();

                    var subject = subjectResponse.Models.FirstOrDefault();

                    subjects.Add(new
                    {
                        SubjectTutorId = st.SubjectTutorId,
                        SubjectId = st.SubjectId,
                        ApprovedAt = st.ApprovedAt,
                        ApprovedBy = st.ApprovedBy,
                        SubjectCode = subject?.SubjectCode ?? "",
                        SubjectName = subject?.Name ?? "",
                        SubjectYear = subject?.Year ?? 0
                    });
                }

                return Ok(subjects);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SubjectTutorController] Error (GetSubjectsForTutor): {ex.Message}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // Check if user is a tutor for a specific subject
        [HttpGet("check/{userId}/{subjectId}")]
        public async Task<IActionResult> CheckTutorStatus(int userId, int subjectId)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var response = await client
                    .From<SubjectTutor>()
                    .Select("subject_tutor_id, is_active")
                    .Filter("user_id", Operator.Equals, userId)
                    .Filter("subject_id", Operator.Equals, subjectId)
                    .Filter("is_active", Operator.Equals, true)
                    .Get();

                var isTutor = response.Models.Any();

                return Ok(new { isTutor = isTutor });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SubjectTutorController] Error (CheckTutorStatus): {ex.Message}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // Deactivate a tutor for a specific subject (admin only)
        [HttpPost("{subjectTutorId}/deactivate")]
        public async Task<IActionResult> DeactivateTutor(int subjectTutorId)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var updateResponse = await client
                    .From<SubjectTutor>()
                    .Set(x => x.IsActive, false)
                    .Filter("subject_tutor_id", Operator.Equals, subjectTutorId)
                    .Update();

                return Ok(new { success = true, message = "Tutor deactivated successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SubjectTutorController] Error (DeactivateTutor): {ex.Message}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // Cleanup method to fix data integrity issues
        [HttpPost("cleanup")]
        public async Task<IActionResult> CleanupTutorData()
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // Get all tutor applications to see what subjects each user actually applied for
                var applicationsResponse = await client
                    .From<TutorApplication>()
                    .Select("user_id, subject_id, status")
                    .Filter("status", Operator.Equals, "approved")
                    .Get();

                var approvedApplications = applicationsResponse.Models
                    .Where(app => app.SubjectId.HasValue)
                    .GroupBy(app => app.UserId)
                    .ToDictionary(g => g.Key, g => g.Select(app => app.SubjectId.Value).ToList());

                Console.WriteLine($"[SubjectTutorController] Found {approvedApplications.Count} users with approved applications");

                // For each user, remove tutor entries for subjects they didn't apply for
                foreach (var userApplications in approvedApplications)
                {
                    var userId = userApplications.Key;
                    var approvedSubjectIds = userApplications.Value;

                    // Get all current tutor entries for this user
                    var tutorEntriesResponse = await client
                        .From<SubjectTutor>()
                        .Select("*")
                        .Filter("user_id", Operator.Equals, userId)
                        .Get();

                    var currentTutorEntries = tutorEntriesResponse.Models.ToList();
                    Console.WriteLine($"[SubjectTutorController] User {userId} has {currentTutorEntries.Count} tutor entries, approved for {approvedSubjectIds.Count} subjects");

                    // Remove entries for subjects they didn't apply for
                    var entriesToRemove = currentTutorEntries
                        .Where(entry => !approvedSubjectIds.Contains(entry.SubjectId))
                        .ToList();

                    foreach (var entry in entriesToRemove)
                    {
                        Console.WriteLine($"[SubjectTutorController] Removing incorrect entry: UserId={userId}, SubjectId={entry.SubjectId}");
                        await client
                            .From<SubjectTutor>()
                            .Filter("subject_tutor_id", Operator.Equals, entry.SubjectTutorId)
                            .Delete();
                    }
                }

                return Ok(new { message = "Tutor data cleanup completed" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SubjectTutorController] Error (CleanupTutorData): {ex.Message}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}
