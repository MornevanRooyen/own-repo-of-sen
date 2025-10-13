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

        public async Task LogoutAsync()
        {
            await _js.InvokeVoidAsync("localStorage.clear");
        }
    }
}
