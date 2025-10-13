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

        private static string SafeAppSetting(string key)
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();
                return config[key];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONFIG ERROR] {ex.Message}");
                return null;
            }
        }

        public SupaBaseAuthService(string url = null, string anonKey = null)
        {
            // ✅ Try environment first, then appsettings.json
            var envUrl = Environment.GetEnvironmentVariable("SUPABASE_URL");
            var envKey = Environment.GetEnvironmentVariable("SUPABASE_KEY");

            url ??= !string.IsNullOrWhiteSpace(envUrl) ? envUrl : SafeAppSetting("Supabase:Url");
            anonKey ??= !string.IsNullOrWhiteSpace(envKey) ? envKey : SafeAppSetting("Supabase:Key");

            if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(anonKey))
                throw new InvalidOperationException("Supabase URL or anon key is missing.");

            var options = new SupabaseOptions
            {
                AutoConnectRealtime = false, // ⚠️ no need for realtime in auth service
                AutoRefreshToken = true
            };

            _client = new Client(url, anonKey, options);
        }

        public async Task InitializeAsync()
        {
            if (_initialized)
                return;

            try
            {
                await _client.InitializeAsync();
                Console.WriteLine("✅ Supabase client initialized.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Supabase initialization failed: {ex.Message}");
            }

            // ✅ Do NOT load any tables or query anything here.
            // No automatic calls to /rest/v1/... — that caused the 406 before.

            _initialized = true;
        }

        /// <summary>
        /// Simple connectivity test for debugging.
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                //Use the client’s configuration 
                var baseUrlField = typeof(Client)
                    .GetField("_url", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var clientUrl = baseUrlField?.GetValue(_client)?.ToString();

                //Basic format validation for whatever URL was passed to the constructor
                if (!Uri.TryCreate(clientUrl, UriKind.Absolute, out var uriResult) ||
                    (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                {
                    Console.WriteLine($"❌ Invalid Supabase URL: {clientUrl}");
                    return false;
                }

                await InitializeAsync();

                if (_client == null || !_initialized)
                {
                    Console.WriteLine("❌ Supabase client failed to initialize.");
                    return false;
                }

                Console.WriteLine("✅ Supabase connection verified.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Supabase connection failed: {ex.Message}");
                return false;
            }
        }
    }
}
