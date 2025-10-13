using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Supabase;
using Sen381.Business.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sen381Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VerifyController : ControllerBase
    {
        private readonly Client _client;

        // ✅ Dependency injection for configuration
        public VerifyController(IConfiguration configuration)
        {
            var url = configuration["Supabase:Url"];
            var key = configuration["Supabase:Key"];

            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("❌ Supabase URL or key is missing in appsettings.json.");

            // ✅ Explicitly set schema to 'public'
            _client = new Client(url, key, new SupabaseOptions
            {
                Schema = "public",
                AutoRefreshToken = true
            });
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest(new { error = "Missing token" });

            await _client.InitializeAsync();

            // ✅ Hash the token using SHA256 (same as Register.cs)
            string tokenHash;
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
                tokenHash = BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }

            Console.WriteLine($"🔍 Received token: {token}");
            Console.WriteLine($"🔍 Computed tokenHash: {tokenHash}");

            // ✅ Query Supabase for the token
            var tokenResponse = await _client
                .From<EmailVerificationToken>()
                .Where(x => x.TokenHash == tokenHash)
                .Get();

            if (!tokenResponse.Models.Any())
            {
                Console.WriteLine("❌ Invalid or expired token.");
                return BadRequest(new { error = "Invalid or expired token" });
            }

            var tokenEntry = tokenResponse.Models.First();

            if (tokenEntry.IsExpired())
            {
                Console.WriteLine("⚠️ Token has expired.");
                return BadRequest(new { error = "Token has expired." });
            }

            // ✅ Fetch user associated with the token
            var userResponse = await _client
                .From<User>()
                .Where(x => x.Id == tokenEntry.UserId)
                .Get();

            if (!userResponse.Models.Any())
            {
                Console.WriteLine("❌ User not found for token.");
                return BadRequest(new { error = "User not found." });
            }

            var user = userResponse.Models.First();

            // ✅ Mark user verified
            user.IsEmailVerified = true;
            await _client.From<User>().Update(user);
            Console.WriteLine($"✅ User '{user.Email}' marked as verified.");

            // ✅ Mark token as used
            tokenEntry.MarkUsed();
            await _client.From<EmailVerificationToken>().Update(tokenEntry);

            Console.WriteLine($"✅ Verification complete for token {tokenEntry.Id} at {DateTime.UtcNow}.");

            // ✅ Redirect user to frontend login page
            return Redirect("https://localhost:7097/login?verified=true");
        }
    }
}
