using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sen381.Business.Models;
using Sen381.Data_Access;

namespace Sen381.Business.Services
{
    public class UserService : IUserService
    {
        private readonly SupaBaseAuthService _supabaseService;

        public UserService(SupaBaseAuthService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        //Fetch user by ID
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            await _supabaseService.InitializeAsync();

            var response = await _supabaseService.Client
                .From<User>()
                .Filter("user_id", Supabase.Postgrest.Constants.Operator.Equals, userId.ToString())
                .Single();

            return response;
        }

        //Update User Profile
        public async Task<bool> UpdateUserAsync(User user)
        {
            await _supabaseService.InitializeAsync();

            var response = await _supabaseService.Client
                .From<User>()
                .Update(user);

            return response.Models.Count > 0;
        }

        public async Task<List<User>> SearchPeersAsync(string query)
        {
            await _supabaseService.InitializeAsync();
            var client = _supabaseService.Client;

            var allUsers = await client
                .From<User>()
                .Select("*")
                .Where(u => u.FirstName.ToLower().Contains(query.ToLower()) ||
                            u.LastName.ToLower().Contains(query.ToLower()))
                .Get();

            return allUsers.Models;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            await _supabaseService.InitializeAsync();
            var client = _supabaseService.Client;

            var response = await client
                .From<User>()
                .Select("*")
                .Get();

            return response.Models;
        }

    }
}
