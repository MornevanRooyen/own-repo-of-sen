using System;
using System.Threading.Tasks;
using Sen381.Business.Models;
using Sen381.Data_Access;

namespace Sen381
{
    public class Login
    {
        private readonly SupaBaseAuthService _supabaseService;

        public Login(SupaBaseAuthService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        public async Task StartLoginAsync()
        {
            Console.WriteLine("Enter your email:");
            string email = Console.ReadLine();

            Console.WriteLine("Enter your password:");
            string password = Console.ReadLine();

            DateTime currentTime = DateTime.UtcNow; // use UTC for consistency

            await _supabaseService.InitializeAsync();

            // Query the database for the user with that email
            var response = await _supabaseService.Client
                .From<User>()
                .Where(u => u.Email == email)
                .Single();

            if (response == null)
            {
                Console.WriteLine("❌ No user found with that email.");
                return;
            }

            // Verify password
            if (response.VerifyPassword(password))
            {
                Console.WriteLine($"✅ User {response.FirstName} {response.LastName} logged in!");

                // Update last_login field in the database
                response.LastLogin = currentTime;
                await _supabaseService.Client
                    .From<User>()
                    .Update(response);

                Console.WriteLine($"📌 last_login updated to {currentTime}.");
            }
            else
            {
                Console.WriteLine("❌ Incorrect password.");
            }
        }
    }
}
