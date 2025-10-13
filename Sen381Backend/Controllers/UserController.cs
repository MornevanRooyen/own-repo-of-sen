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
    public class UserController : ControllerBase
    {
        private readonly SupaBaseAuthService _supabase;

        public UserController(SupaBaseAuthService supabase)
        {
            _supabase = supabase;
        }

        // ✅ Get user by email (used during login, optional fallback)
        [HttpGet("{email}")]
        public async Task<IActionResult> GetUserInfo(string email)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var response = await client
                    .From<User>()
                    .Select("first_name, last_name, role")
                    .Filter("email", Operator.Equals, email)
                    .Get();

                var user = response.Models.FirstOrDefault();

                if (user == null)
                    return NotFound(new { error = "User not found" });

                return Ok(new
                {
                    fullName = $"{user.FirstName} {user.LastName}",
                    role = user.RoleString
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserController] Error (GetUserInfo): {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        // ✅ NEW endpoint: Get user by ID (for dashboard + profile)
        [HttpGet("by-id/{userId:int}")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var response = await client
                    .From<User>()
                    .Select("first_name, last_name, role")
                    .Filter("user_id", Operator.Equals, userId)
                    .Get();

                var user = response.Models.FirstOrDefault();

                if (user == null)
                    return NotFound(new { error = "User not found" });

                // ✅ Return clean JSON object
                return Ok(new
                {
                    fullName = $"{user.FirstName} {user.LastName}",
                    role = user.RoleString
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserController] Error (GetUserById): {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }
    }
}
