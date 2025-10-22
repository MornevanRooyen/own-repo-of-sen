using Microsoft.AspNetCore.Mvc;
using Sen381;
using Sen381.Business.Models;
using Sen381.Data_Access;
using Supabase.Postgrest;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace Sen381Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly SupaBaseAuthService _supabase;
        private readonly HttpClient _httpClient;

        public AuthController(SupaBaseAuthService supabase)
        {
            _supabase = supabase;
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// POST /api/Auth/login
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            Console.WriteLine($"[LOGIN ATTEMPT] Email='{request.Email}', Password='******'");

            // === Basic validation ===
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Email and password are required.");

            if (!request.Email.Contains("@"))
                return BadRequest("Invalid email format.");

            try
            {
                await _supabase.InitializeAsync();
                var client = _supabase.Client;

                // === Hash entered password (SHA256 to match stored hash) ===
                using var sha = SHA256.Create();
                var hashedBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(request.Password));
                var hashedPassword = BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();

                // === Get user record from your Users table ===
                var response = await client
                    .From<User>()
                    .Select("*")
                    .Where(u => u.Email == request.Email)
                    .Get();

                var user = response.Models.FirstOrDefault();
                if (user == null)
                {
                    Console.WriteLine($"[LOGIN FAILED] Email not found: {request.Email}");
                    return Unauthorized("Incorrect email or password.");
                }

                if (!string.Equals(user.PasswordHash, hashedPassword, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"[LOGIN FAILED] Incorrect password for: {request.Email}");
                    return Unauthorized("Incorrect email or password.");
                }

                if (!user.IsEmailVerified)
                {
                    Console.WriteLine($"[LOGIN FAILED] Unverified email: {request.Email}");
                    return Unauthorized("Email not verified. Please verify your email before logging in.");
                }

                // Update LastLogin timestamp
                try
                {
                    var now = DateTime.UtcNow;
                    var result = await client
                        .From<User>()
                        .Filter("user_id", Operator.Equals, user.Id)
                        .Set(u => u.LastLogin, now)
                        .Update();

                    Console.WriteLine($"[LOGIN INFO] Updated LastLogin for user {user.Email} → {now}");
                }
                catch (Exception updateEx)
                {
                    Console.WriteLine($"[LOGIN WARNING] Could not update LastLogin: {updateEx.Message}");
                }

                Console.WriteLine($"[LOGIN SUCCESS] User '{request.Email}' logged in successfully.");

                return Ok(new
                {
                    message = "Login successful",
                    userId = user.Id,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    role = user.RoleString,
                    phoneNum = user.PhoneNum,
                    program = user.Program,
                    year = user.Year
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOGIN ERROR] {ex.Message}");
                return StatusCode(500, $"Unexpected error: {ex.Message}");
            }
        }

        /// <summary>
        /// POST /api/Auth/register
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            Console.WriteLine($"[REGISTER ATTEMPT] Email='{request.Email}', Name='{request.FirstName} {request.LastName}'");

            try
            {
                // ✅ 1. Basic input validation
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                    return BadRequest("Email and password are required.");

                if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
                    return BadRequest("First name and last name are required.");

                if (!request.Email.Contains("@"))
                    return BadRequest("Invalid email format.");

                if (request.Password.Length < 6)
                    return BadRequest("Password must be at least 6 characters.");

                await _supabase.InitializeAsync();

                // ✅ 2. Check if user already exists
                var existingUsers = await _supabase.Client
                    .From<User>()
                    .Where(u => u.Email == request.Email)
                    .Get();

                if (existingUsers.Models.Any())
                    return BadRequest("A user with this email already exists.");

                // ✅ 3. Create the new user object
                var user = new User
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNum = "", // Set default or from request if available
                    Email = request.Email,
                    Program = request.Program,
                    Year = request.Year,
                    ProfilePicturePath = "",
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow,
                    IsEmailVerified = false
                };

                user.SetRole(Role.student);
                user.ChangePassword(request.Password);
                user.Id = default; // prevent sending user_id=0

                // ✅ 4. Insert into Supabase
                await _supabase.Client
                    .From<User>()
                    .Insert(user, new QueryOptions
                    {
                        Returning = QueryOptions.ReturnType.Minimal
                    });

                // ✅ 5. Re-fetch record to get auto-generated ID
                var fetchResponse = await _supabase.Client
                    .From<User>()
                    .Where(u => u.Email == request.Email)
                    .Get();

                var insertedUser = fetchResponse.Models.FirstOrDefault();
                if (insertedUser == null)
                    return StatusCode(500, "Failed to retrieve newly created user record.");

                // ✅ 6. Create and store verification token
                string rawToken = await CreateVerificationTokenAsync(insertedUser);

                // ✅ 7. Send verification email
                await SendVerificationEmailAsync(insertedUser.Email, rawToken);

                Console.WriteLine($"✅ User '{insertedUser.Email}' registered successfully with ID {insertedUser.Id}.");

                return Ok(new
                {
                    message = "Registration successful! Please check your email for verification.",
                    userId = insertedUser.Id,
                    email = insertedUser.Email
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Registration failed: {ex.Message}");
                return StatusCode(500, $"Unexpected error occurred: {ex.Message}");
            }
        }

        // 🔑 Create & store email verification token
        private async Task<string> CreateVerificationTokenAsync(User user)
        {
            string rawToken = Guid.NewGuid().ToString();
            string tokenHash = HashToken(rawToken);

            var token = new EmailVerificationToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            token.Id = default; // prevent sending email_verification_token_id=0

            await _supabase.Client
                .From<EmailVerificationToken>()
                .Insert(token, new QueryOptions
                {
                    Returning = QueryOptions.ReturnType.Minimal
                });

            Console.WriteLine($"🔑 Verification token stored for {user.Email}");
            return rawToken;
        }

        // 📧 Send verification email via backend API
        private async Task SendVerificationEmailAsync(string email, string token)
        {
            try
            {
                const string apiUrl = "https://localhost:7228/api/email/send-verification";
                var response = await _httpClient.PostAsJsonAsync(apiUrl, new { Email = email, Token = token });

                if (response.IsSuccessStatusCode)
                    Console.WriteLine($"📧 Verification email sent to {email}");
                else
                    Console.WriteLine($"❌ Failed to send verification email: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error calling backend email API: {ex.Message}");
            }
        }

        // 🔒 Securely hash token using SHA256
        private string HashToken(string token)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }

    // Request DTOs
    public class LoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public bool RememberMe { get; set; } = false;
    }

    public class RegisterRequest
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string? Program { get; set; }
        public string? Year { get; set; }
    }
}

