using Microsoft.AspNetCore.Mvc;
using Sen381.Data_Access;
using Sen381.Business.Models;
using System.Security.Cryptography;
using System.Text;

namespace Sen381Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PasswordFixController : ControllerBase
    {
        private readonly SupaBaseAuthService _supabase;

        public PasswordFixController(SupaBaseAuthService supabase)
        {
            _supabase = supabase;
        }

        [HttpPost("rehash")]
        public async Task<IActionResult> RehashPasswords()
        {
            await _supabase.InitializeAsync();
            var client = _supabase.Client;

            var response = await client
                .From<User>()
                .Select("*")
                .Get();

            int updated = 0;

            foreach (var user in response.Models)
            {
                string current = user.PasswordHash ?? "";

                // Skip if already SHA256 (64 hex chars)
                if (System.Text.RegularExpressions.Regex.IsMatch(current, @"^[0-9a-f]{64}$"))
                    continue;

                try
                {
                    // Try Base64 decode, else treat as plain text
                    byte[] decoded;
                    try
                    {
                        decoded = Convert.FromBase64String(current);
                    }
                    catch
                    {
                        decoded = Encoding.UTF8.GetBytes(current);
                    }

                    using var sha = SHA256.Create();
                    var hash = sha.ComputeHash(decoded);
                    var hashedPassword = BitConverter.ToString(hash).Replace("-", "").ToLower();

                    user.PasswordHash = hashedPassword;
                    await client.From<User>().Upsert(user);
                    updated++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Failed for {user.Email}: {ex.Message}");
                }
            }

            return Ok(new { updated });
        }
    }
}
