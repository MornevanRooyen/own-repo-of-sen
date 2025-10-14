using Microsoft.Extensions.Configuration;
using Supabase;
using System;
using System.Threading.Tasks;

namespace Sen381.Data_Access
{
    public class SupaBaseAuthService
    {
        private readonly Client _client;
        public Client Client => _client;
        private bool _initialized;

        public SupaBaseAuthService(IConfiguration config)
        {
            var url = config["Supabase:Url"];
            var key = config["Supabase:Key"] ?? config["Supabase:AnonKey"];

            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("Supabase configuration missing: Url or Key not found in appsettings.json.");

            _client = new Client(url, key, new SupabaseOptions
            {
                AutoRefreshToken = true,
                Schema = "public"
            });

            Console.WriteLine($"[DEBUG] Supabase client created for {url}");
        }

        public async Task InitializeAsync()
        {
            if (_initialized) return;
            _initialized = true;
            Console.WriteLine("✅ Supabase client initialized.");
            await Task.CompletedTask;
        }
    }
}
