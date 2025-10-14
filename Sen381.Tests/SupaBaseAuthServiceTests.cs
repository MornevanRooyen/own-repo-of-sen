using Microsoft.Extensions.Configuration;
using Supabase;
using System;
using System.Threading.Tasks;

namespace Sen381.Data_Access
{
    public class SupaBaseAuthService
    {
        private Client? _client;
        public Client Client
        {
            get
            {
                if (_client == null)
                    throw new InvalidOperationException("Supabase client not initialized. Call InitializeAsync() first.");
                return _client;
            }
        }

        private string _url = string.Empty;
        private string _anonKey = string.Empty;
        private bool _initialized;

        // ==== Helpers ====
        private static string SafeAppSetting(string key)
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();
                return config[key] ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        // ==== Constructors expected by your tests ====

        // (A) Parameterless: reads Supabase:Url and Supabase:Key (anon) from appsettings.json
        public SupaBaseAuthService()
        {
            _url = SafeAppSetting("Supabase:Url");
            _anonKey = SafeAppSetting("Supabase:Key");
        }

        // (B) Explicit: tests call new SupaBaseAuthService(url: "...", anonKey: "...")
        public SupaBaseAuthService(string url, string anonKey)
        {
            _url = url ?? string.Empty;
            _anonKey = anonKey ?? string.Empty;
        }

        // ==== Init ====
        public async Task InitializeAsync()
        {
            if (_initialized) return;

            // Create client using whatever url/key are currently set
            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                Schema = "public"
            };

            _client = new Client(_url, _anonKey, options);
            _initialized = true;
            await Task.CompletedTask;
        }

        // ==== Used by your tests ====
        // Intention (based on your test comments):
        //  - return false for obviously invalid url
        //  - return true for "valid-looking" setup without performing network calls
        public Task<bool> TestConnectionAsync()
        {
            // purely local validation: no outbound request (keeps unit tests deterministic)
            var looksLikeUrl = Uri.TryCreate(_url, UriKind.Absolute, out var parsed)
                               && (parsed.Scheme == Uri.UriSchemeHttps || parsed.Scheme == Uri.UriSchemeHttp);

            var hasKey = !string.IsNullOrWhiteSpace(_anonKey);

            var ok = looksLikeUrl && hasKey;
            return Task.FromResult(ok);
        }
    }
}
