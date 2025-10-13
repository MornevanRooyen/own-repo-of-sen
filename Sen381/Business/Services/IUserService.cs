using System.Collections.Generic;
using System.Threading.Tasks;
using Sen381.Business.Models;

namespace Sen381.Business.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
    }
}
