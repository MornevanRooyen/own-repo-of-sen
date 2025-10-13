using Sen381.Business.Models;
using Sen381.Data_Access;
using Supabase.Postgrest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sen381
{
    public class Register
    {
        private readonly List<User> _users = new();
        private readonly SupaBaseAuthService _supabaseService;
        private readonly HttpClient _httpClient;

        public Register(SupaBaseAuthService supabaseService)
        {
            _supabaseService = supabaseService;
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Registers a new user in the database and sends an email verification link.
        /// Returns a detailed RegistrationResult indicating success/failure reason.
        /// </summary>
        public async Task<RegistrationResult> StartRegisterAsync(RegisterModel model)
        {
            try
            {
                // ✅ 1. Basic input validation
                if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                    return new RegistrationResult { Success = false, Message = "Email and password are required." };

                if (model.Password != model.ConfirmPassword)
                    return new RegistrationResult { Success = false, Message = "Passwords do not match." };

                await _supabaseService.InitializeAsync();

                // ✅ 2. Check if user already exists
                var existingUsers = await _supabaseService.Client
                    .From<User>()
                    .Where(u => u.Email == model.Email)
                    .Get();

                if (existingUsers.Models.Any())
                    return new RegistrationResult { Success = false, Message = "A user with this email already exists." };

                // ✅ 3. Create the new user object
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNum = model.PhoneNum,
                    Email = model.Email,
                    ProfilePicturePath = "",
                    CreatedAt = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow,
                    IsEmailVerified = false
                };

                user.SetRole(Role.student);
                user.ChangePassword(model.Password);
                user.Id = default; // prevent sending user_id=0

                // ✅ 4. Insert into Supabase
                await _supabaseService.Client
                    .From<User>()
                    .Insert(user, new QueryOptions
                    {
                        Returning = QueryOptions.ReturnType.Minimal
                    });

                // ✅ 5. Re-fetch record to get auto-generated ID
                var fetchResponse = await _supabaseService.Client
                    .From<User>()
                    .Where(u => u.Email == model.Email)
                    .Get();

                var insertedUser = fetchResponse.Models.FirstOrDefault();
                if (insertedUser == null)
                    return new RegistrationResult { Success = false, Message = "Failed to retrieve newly created user record." };

                // ✅ 6. Create and store verification token
                string rawToken = await CreateVerificationTokenAsync(insertedUser);

                // ✅ 7. Send verification email
                await SendVerificationEmailAsync(insertedUser.Email, rawToken);

                Console.WriteLine($"✅ User '{insertedUser.Email}' registered successfully with ID {insertedUser.Id}.");

                return new RegistrationResult
                {
                    Success = true,
                    Message = "Registration successful! Please check your email for verification."
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Registration failed: {ex.Message}");
                return new RegistrationResult
                {
                    Success = false,
                    Message = $"Unexpected error occurred: {ex.Message}"
                };
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

            await _supabaseService.Client
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

        // 👀 Debugging utility
        public void DisplayAllUsers()
        {
            if (_users.Count == 0)
            {
                Console.WriteLine("No users registered yet.");
                return;
            }

            foreach (var user in _users)
            {
                Console.WriteLine($"ID: {user.Id}, Name: {user.FirstName} {user.LastName}, " +
                                  $"Email: {user.Email}, Verified: {user.IsEmailVerified}, Role: {user.GetRole()}");
            }
        }
    }
}
