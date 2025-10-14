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
    public class ProfileController : ControllerBase
    {
        private readonly SupaBaseAuthService _supabase;

        public ProfileController(SupaBaseAuthService supabase)
        {
            _supabase = supabase;
        }

        // =============================
        // GET profile by user ID (alias route for /api/Profile/{id})
        // =============================
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByIdAlias(int id)
        {
            // ✅ Just delegate to the main handler below
            return await GetById(id);
        }

        // =============================
        // GET profile by user ID (main route)
        // =============================
        [HttpGet("by-id/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            Console.WriteLine($"[ProfileController] ===== START FETCH by-id/{id} =====");

            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                var res = await client
                    .From<User>()
                    .Select("*")
                    .Filter("user_id", Operator.Equals, id)
                    .Get();

                var u = res.Models.FirstOrDefault();
                if (u == null)
                {
                    Console.WriteLine($"⚠️ No user found with ID {id}");
                    return NotFound(new { error = "User not found" });
                }

                var dto = new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.PhoneNum,
                    u.City,
                    u.Country,
                    u.Timezone,
                    u.Website,
                    u.Program,
                    u.Year,
                    u.About,
                    u.ContactPreference,
                    u.Interests,
                    u.Subjects,
                    u.RoleString,
                    u.ProfilePicturePath
                };

                Console.WriteLine($"✅ User found: {u.FirstName} {u.LastName}");
                return Ok(dto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProfileController:GetById] {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // =============================
        // UPDATE profile (non-system fields only)
        // =============================
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProfileUpdateDto updateDto)
        {
            Console.WriteLine($"[ProfileController] ===== START UPDATE user_id={id} =====");

            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // ✅ 1. Fetch existing user
                var existingResponse = await client
                    .From<User>()
                    .Select("*")
                    .Filter("user_id", Operator.Equals, id)
                    .Single();

                var existingUser = existingResponse ?? throw new Exception("User not found.");

                // ✅ 2. Merge editable fields with preserved ones
                var updatedUser = new User
                {
                    Id = id,
                    // Editable fields
                    FirstName = updateDto.FirstName,
                    LastName = updateDto.LastName,
                    PhoneNum = updateDto.PhoneNum,
                    City = updateDto.City,
                    Country = updateDto.Country,
                    Timezone = updateDto.Timezone,
                    Website = updateDto.Website,
                    Program = updateDto.Program,
                    Year = updateDto.Year,
                    About = updateDto.About,
                    ContactPreference = updateDto.ContactPreference,
                    Interests = updateDto.Interests,
                    Subjects = updateDto.Subjects,
                    Email = updateDto.Email,
                    ProfilePicturePath = updateDto.ProfilePicturePath,

                    // Preserved fields
                    PasswordHash = existingUser.PasswordHash,
                    IsEmailVerified = existingUser.IsEmailVerified,
                    CreatedAt = existingUser.CreatedAt,
                    LastLogin = existingUser.LastLogin,
                    RoleString = existingUser.RoleString
                };

                // ✅ 3. Update safely
                await client
                    .From<User>()
                    .Filter("user_id", Operator.Equals, id)
                    .Update(updatedUser);

                Console.WriteLine($"✅ Profile updated successfully for user_id={id}");
                return Ok(new { message = "Profile updated successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [ProfileController:Update] {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // =============================
    // DTO for profile updates
    // =============================
    public class ProfileUpdateDto
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNum { get; set; } = "";
        public string City { get; set; } = "";
        public string Country { get; set; } = "";
        public string Timezone { get; set; } = "";
        public string Website { get; set; } = "";
        public string Program { get; set; } = "";
        public string Year { get; set; } = "";
        public string About { get; set; } = "";
        public string ContactPreference { get; set; } = "";
        public string Interests { get; set; } = "";
        public string Subjects { get; set; } = "";
        public string ProfilePicturePath { get; set; } = "";
    }
}
