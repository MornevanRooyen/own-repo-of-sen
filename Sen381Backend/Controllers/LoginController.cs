using Microsoft.AspNetCore.Mvc;
using Sen381.Business.Models;
using Sen381.Data_Access;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace Sen381Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly SupaBaseAuthService _supabase;

        public LoginController(SupaBaseAuthService supabase)
        {
            _supabase = supabase;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            Console.WriteLine($"[LOGIN ATTEMPT] Email='{model.Email}', Password='******'");

            // === Basic validation ===
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { error = "Email and password are required." });

            if (!model.Email.Contains("@"))
                return BadRequest(new { error = "Invalid email format." });

            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // === Hash entered password (SHA256 to match stored hash) ===
                using var sha = SHA256.Create();
                var hashedBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(model.Password));
                var hashedPassword = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();

                // === Get user record from your Users table ===
                var response = await client
                    .From<User>()
                    .Select("*")
                    .Where(u => u.Email == model.Email)
                    .Get();

                var user = response.Models.FirstOrDefault();
                if (user == null)
                {
                    Console.WriteLine($"[LOGIN FAILED] Email not found: {model.Email}");
                    return Unauthorized(new { error = "Email not found." });
                }

                if (!string.Equals(user.PasswordHash, hashedPassword, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"[LOGIN FAILED] Incorrect password for: {model.Email}");
                    return Unauthorized(new { error = "Incorrect password." });
                }

                if (!user.IsEmailVerified)
                {
                    Console.WriteLine($"[LOGIN FAILED] Unverified email: {model.Email}");
                    return Unauthorized(new { error = "Email not verified. Please verify your email before logging in." });
                }

                try
                {
                    var now = DateTime.UtcNow;

                    var result = await client
                        .From<User>()
                        .Filter("user_id", Operator.Equals, user.Id)
                        .Set(u => u.LastLogin, now)  // ✅ use .Set() for partial column updates
                        .Update();

                    Console.WriteLine($"[LOGIN INFO] Updated LastLogin for user {user.Email} → {now}");
                    Console.WriteLine($"[LOGIN DEBUG] Rows affected: {result.Models.Count}");
                }
                catch (Exception updateEx)
                {
                    Console.WriteLine($"[LOGIN WARNING] Could not update LastLogin: {updateEx.Message}");
                }


                // ✅ Skip Supabase Auth session creation — not needed for custom auth
                Console.WriteLine($"[LOGIN SUCCESS] User '{model.Email}' logged in successfully.");

                return Ok(new
                {
                    message = "Login successful",
                    userId = user.Id,
                    email = user.Email,
                    role = user.RoleString
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOGIN ERROR] {ex.Message}");
                return StatusCode(500, new { error = $"Unexpected error: {ex.Message}" });
            }
        }
    }
}
