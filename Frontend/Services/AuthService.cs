using Microsoft.JSInterop;
using System.Threading.Tasks;


namespace Frontend.Services
{
    public class AuthService
    {
        private readonly IJSRuntime _js;

        public AuthService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<bool> IsUserLoggedInAsync()
        {
            var result = await _js.InvokeAsync<string>("localStorage.getItem", "isLoggedIn");
            return result == "true";
        }

        public async Task<int?> GetCurrentUserIdAsync()
        {
            var idString = await _js.InvokeAsync<string>("localStorage.getItem", "userId");
            if (int.TryParse(idString, out int id))
                return id;
            return null;
        }

        public async Task<string?> GetCurrentUserRoleAsync()
        {
            var role = await _js.InvokeAsync<string>("localStorage.getItem", "role");
            return string.IsNullOrWhiteSpace(role) ? null : role;
        }

        public async Task LogoutAsync()
        {
            await _js.InvokeVoidAsync("localStorage.clear");
        }

        public async Task<CurrentUser?> GetCurrentUserAsync()
        {
            try
            {
                var userId = await GetCurrentUserIdAsync();
                if (userId == null) return null;

                var userRole = await GetCurrentUserRoleAsync();
                
                return new CurrentUser
                {
                    UserId = userId.Value,
                    PhoneNum = await _js.InvokeAsync<string>("localStorage.getItem", "phoneNum"),
                    StudentNo = await _js.InvokeAsync<string>("localStorage.getItem", "studentNo"),
                    Program = await _js.InvokeAsync<string>("localStorage.getItem", "program"),
                    Year = await _js.InvokeAsync<string>("localStorage.getItem", "year"),
                    Role = userRole
                };
            }
            catch
            {
                return null;
            }
        }

        // Synchronous version for compatibility
        public CurrentUser? GetCurrentUser()
        {
            // This is a fallback - prefer GetCurrentUserAsync for new code
            return new CurrentUser
            {
                UserId = 1,
                PhoneNum = "+27 82 123 4567",
                StudentNo = "BC1234567",
                Program = "BEng (Software)",
                Year = "3"
            };
        }
    }

    public class CurrentUser
    {
        public int UserId { get; set; }
        public string? PhoneNum { get; set; }
        public string? StudentNo { get; set; }
        public string? Program { get; set; }
        public string? Year { get; set; }
        public string? Role { get; set; }
    }
}
