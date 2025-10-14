using Microsoft.AspNetCore.Mvc;
using Sen381.Business.Models;
using Sen381.Data_Access;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace Sen381Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FollowController : ControllerBase
    {
        private readonly SupaBaseAuthService _supabase;

        public FollowController(SupaBaseAuthService supabase)
        {
            _supabase = supabase;
        }

        // =============================
        // POST: Follow a user
        // =============================
        [HttpPost("follow")]
        public async Task<IActionResult> FollowUser([FromBody] FollowRequest request)
        {
            Console.WriteLine($"[FollowController] Follow request: {request.FollowerId} -> {request.FollowingId}");

            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // Check if already following
                var existing = await client
                    .From<UserFollow>()
                    .Select("*")
                    .Filter("follower_id", Operator.Equals, request.FollowerId)
                    .Filter("following_id", Operator.Equals, request.FollowingId)
                    .Get();

                if (existing.Models.Any())
                {
                    return BadRequest(new { error = "Already following this user" });
                }

                // Create new follow relationship
                var follow = new UserFollow
                {
                    FollowerId = request.FollowerId,
                    FollowingId = request.FollowingId
                };

                var response = await client
                    .From<UserFollow>()
                    .Insert(follow);

                Console.WriteLine($"✅ Follow created: {request.FollowerId} -> {request.FollowingId}");
                return Ok(new { success = true, message = "Successfully followed user" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error following user: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // =============================
        // POST: Unfollow a user
        // =============================
        [HttpPost("unfollow")]
        public async Task<IActionResult> UnfollowUser([FromBody] FollowRequest request)
        {
            Console.WriteLine($"[FollowController] Unfollow request: {request.FollowerId} -> {request.FollowingId}");

            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // Find the follow relationship
                var existing = await client
                    .From<UserFollow>()
                    .Select("*")
                    .Filter("follower_id", Operator.Equals, request.FollowerId)
                    .Filter("following_id", Operator.Equals, request.FollowingId)
                    .Get();

                var follow = existing.Models.FirstOrDefault();
                if (follow == null)
                {
                    return BadRequest(new { error = "Not following this user" });
                }

                // Delete the follow relationship
                await client
                    .From<UserFollow>()
                    .Where(f => f.FollowerId == request.FollowerId && f.FollowingId == request.FollowingId)
                    .Delete();

                Console.WriteLine($"✅ Unfollow successful: {request.FollowerId} -> {request.FollowingId}");
                return Ok(new { success = true, message = "Successfully unfollowed user" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error unfollowing user: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // =============================
        // GET: Check if following a user
        // =============================
        [HttpGet("check/{followerId}/{followingId}")]
        public async Task<IActionResult> CheckFollowing(int followerId, int followingId)
        {
            Console.WriteLine($"[FollowController] Check following: {followerId} -> {followingId}");

            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var existing = await client
                    .From<UserFollow>()
                    .Select("*")
                    .Filter("follower_id", Operator.Equals, followerId)
                    .Filter("following_id", Operator.Equals, followingId)
                    .Get();

                bool isFollowing = existing.Models.Any();
                Console.WriteLine($"✅ Following status: {isFollowing}");

                return Ok(new { isFollowing });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error checking follow status: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // =============================
        // GET: Get followers count
        // =============================
        [HttpGet("followers-count/{userId}")]
        public async Task<IActionResult> GetFollowersCount(int userId)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var followers = await client
                    .From<UserFollow>()
                    .Select("*")
                    .Filter("following_id", Operator.Equals, userId)
                    .Get();

                int count = followers.Models.Count;
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error getting followers count: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // =============================
        // GET: Get following count
        // =============================
        [HttpGet("following-count/{userId}")]
        public async Task<IActionResult> GetFollowingCount(int userId)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var following = await client
                    .From<UserFollow>()
                    .Select("*")
                    .Filter("follower_id", Operator.Equals, userId)
                    .Get();

                int count = following.Models.Count;
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error getting following count: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // =============================
        // GET: Get list of users you're following with their details
        // =============================
        [HttpGet("following-list/{userId}")]
        public async Task<IActionResult> GetFollowingList(int userId)
        {
            Console.WriteLine($"[FollowController] Getting following list for user {userId}");

            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // Get all user_follows records where this user is the follower
                var followsResponse = await client
                    .From<UserFollow>()
                    .Select("*")
                    .Filter("follower_id", Operator.Equals, userId)
                    .Get();

                var follows = followsResponse.Models;
                if (!follows.Any())
                {
                    Console.WriteLine($"✅ User {userId} is not following anyone");
                    return Ok(new List<UserDto>());
                }

                // Get the IDs of users being followed
                var followingIds = follows.Select(f => f.FollowingId).ToList();

                // Fetch user details for all followed users
                var users = new List<UserDto>();
                foreach (var followingId in followingIds)
                {
                    var userResponse = await client
                        .From<User>()
                        .Select("*")
                        .Filter("user_id", Operator.Equals, followingId)
                        .Get();

                    var user = userResponse.Models.FirstOrDefault();
                    if (user != null)
                    {
                        users.Add(new UserDto
                        {
                            Id = user.Id,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.Email,
                            PhoneNum = user.PhoneNum,
                            RoleString = user.RoleString,
                            ProfilePicturePath = user.ProfilePicturePath,
                            City = user.City,
                            Country = user.Country,
                            Timezone = user.Timezone,
                            Website = user.Website,
                            Program = user.Program,
                            Year = user.Year,
                            About = user.About,
                            Interests = user.Interests
                        });
                    }
                }

                Console.WriteLine($"✅ Found {users.Count} users that user {userId} is following");
                return Ok(users);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error getting following list: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class FollowRequest
    {
        public int FollowerId { get; set; }
        public int FollowingId { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNum { get; set; }
        public string RoleString { get; set; }
        public string ProfilePicturePath { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Timezone { get; set; }
        public string Website { get; set; }
        public string Program { get; set; }
        public string Year { get; set; }
        public string About { get; set; }
        public string Interests { get; set; }
    }
}

