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
    public class SubjectSubscriptionController : ControllerBase
    {
        private readonly SupaBaseAuthService _supabase;

        public SubjectSubscriptionController(SupaBaseAuthService supabase)
        {
            _supabase = supabase;
        }

        // Subscribe to a subject
        [HttpPost("subscribe")]
        public async Task<IActionResult> SubscribeToSubject([FromBody] SubscriptionRequest request)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // Check if user is already subscribed
                var existingSubscription = await client
                    .From<SubjectSubscription>()
                    .Filter("user_id", Operator.Equals, request.UserId)
                    .Filter("subject_id", Operator.Equals, request.SubjectId)
                    .Get();

                if (existingSubscription.Models.Any())
                {
                    var subscription = existingSubscription.Models.First();
                    if (subscription.IsActive)
                    {
                        return Ok(new SubscriptionResponse 
                        { 
                            Success = false, 
                            Message = "You are already subscribed to this subject",
                            IsSubscribed = true
                        });
                    }
                    else
                    {
                        // Reactivate existing subscription
                        await client
                            .From<SubjectSubscription>()
                            .Set(x => x.IsActive, true)
                            .Set(x => x.SubscribedAt, DateTime.UtcNow)
                            .Filter("subscription_id", Operator.Equals, subscription.SubscriptionId)
                            .Update();

                        return Ok(new SubscriptionResponse 
                        { 
                            Success = true, 
                            Message = "Successfully resubscribed to subject",
                            IsSubscribed = true
                        });
                    }
                }

                // Create new subscription
                var newSubscription = new SubjectSubscription
                {
                    UserId = request.UserId,
                    SubjectId = request.SubjectId,
                    IsActive = true,
                    SubscribedAt = DateTime.UtcNow
                };

                await client.From<SubjectSubscription>().Insert(newSubscription);

                return Ok(new SubscriptionResponse 
                { 
                    Success = true, 
                    Message = "Successfully subscribed to subject",
                    IsSubscribed = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SubjectSubscriptionController] Error (SubscribeToSubject): {ex.Message}");
                return StatusCode(500, new SubscriptionResponse 
                { 
                    Success = false, 
                    Message = "Internal server error",
                    IsSubscribed = false
                });
            }
        }

        // Unsubscribe from a subject
        [HttpPost("unsubscribe")]
        public async Task<IActionResult> UnsubscribeFromSubject([FromBody] SubscriptionRequest request)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // Find existing subscription
                var existingSubscription = await client
                    .From<SubjectSubscription>()
                    .Filter("user_id", Operator.Equals, request.UserId)
                    .Filter("subject_id", Operator.Equals, request.SubjectId)
                    .Get();

                if (!existingSubscription.Models.Any())
                {
                    return Ok(new SubscriptionResponse 
                    { 
                        Success = false, 
                        Message = "You are not subscribed to this subject",
                        IsSubscribed = false
                    });
                }

                var subscription = existingSubscription.Models.First();
                if (!subscription.IsActive)
                {
                    return Ok(new SubscriptionResponse 
                    { 
                        Success = false, 
                        Message = "You are not subscribed to this subject",
                        IsSubscribed = false
                    });
                }

                // Deactivate subscription
                await client
                    .From<SubjectSubscription>()
                    .Set(x => x.IsActive, false)
                    .Filter("subscription_id", Operator.Equals, subscription.SubscriptionId)
                    .Update();

                return Ok(new SubscriptionResponse 
                { 
                    Success = true, 
                    Message = "Successfully unsubscribed from subject",
                    IsSubscribed = false
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SubjectSubscriptionController] Error (UnsubscribeFromSubject): {ex.Message}");
                return StatusCode(500, new SubscriptionResponse 
                { 
                    Success = false, 
                    Message = "Internal server error",
                    IsSubscribed = false
                });
            }
        }

        // Check subscription status
        [HttpGet("status/{userId}/{subjectId}")]
        public async Task<IActionResult> GetSubscriptionStatus(int userId, int subjectId)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var subscription = await client
                    .From<SubjectSubscription>()
                    .Filter("user_id", Operator.Equals, userId)
                    .Filter("subject_id", Operator.Equals, subjectId)
                    .Get();

                bool isSubscribed = subscription.Models.Any(s => s.IsActive);

                return Ok(new SubscriptionResponse 
                { 
                    Success = true, 
                    Message = isSubscribed ? "Subscribed" : "Not subscribed",
                    IsSubscribed = isSubscribed
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SubjectSubscriptionController] Error (GetSubscriptionStatus): {ex.Message}");
                return StatusCode(500, new SubscriptionResponse 
                { 
                    Success = false, 
                    Message = "Internal server error",
                    IsSubscribed = false
                });
            }
        }

        // Get all subscriptions for a user
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserSubscriptions(int userId)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var subscriptions = await client
                    .From<SubjectSubscription>()
                    .Filter("user_id", Operator.Equals, userId)
                    .Filter("is_active", Operator.Equals, true)
                    .Get();

                var result = new List<SubjectSubscriptionDto>();
                foreach (var sub in subscriptions.Models)
                {
                    // Get subject details
                    var subjectResponse = await client
                        .From<Sen381Backend.Models.Subject>()
                        .Select("subject_code, name, year")
                        .Filter("subject_id", Operator.Equals, sub.SubjectId)
                        .Get();

                    var subject = subjectResponse.Models.FirstOrDefault();

                    result.Add(new SubjectSubscriptionDto
                    {
                        SubscriptionId = sub.SubscriptionId,
                        UserId = sub.UserId,
                        SubjectId = sub.SubjectId,
                        SubscribedAt = sub.SubscribedAt,
                        IsActive = sub.IsActive,
                        SubjectCode = subject?.SubjectCode ?? "",
                        SubjectName = subject?.Name ?? "",
                        SubjectYear = subject?.Year ?? 0
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SubjectSubscriptionController] Error (GetUserSubscriptions): {ex.Message}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // Get all subscribers for a subject
        [HttpGet("subject/{subjectId}/subscribers")]
        public async Task<IActionResult> GetSubscribersForSubject(int subjectId)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var subscriptions = await client
                    .From<SubjectSubscription>()
                    .Filter("subject_id", Operator.Equals, subjectId)
                    .Get();

                var activeSubscriptions = subscriptions.Models.Where(s => s.IsActive).ToList();

                var result = new List<SubjectSubscriptionDto>();
                foreach (var sub in activeSubscriptions)
                {
                    // Get user details
                    var userResponse = await client
                        .From<User>()
                        .Select("first_name, last_name, email, profile_picture_path")
                        .Filter("user_id", Operator.Equals, sub.UserId)
                        .Get();

                    var user = userResponse.Models.FirstOrDefault();

                    result.Add(new SubjectSubscriptionDto
                    {
                        SubscriptionId = sub.SubscriptionId,
                        UserId = sub.UserId,
                        SubjectId = sub.SubjectId,
                        SubscribedAt = sub.SubscribedAt,
                        IsActive = sub.IsActive,
                        FirstName = user?.FirstName ?? "",
                        LastName = user?.LastName ?? "",
                        Email = user?.Email ?? "",
                        ProfilePicturePath = user?.ProfilePicturePath
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SubjectSubscriptionController] Error (GetSubscribersForSubject): {ex.Message}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}
