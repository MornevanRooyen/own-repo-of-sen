using Microsoft.AspNetCore.Mvc;
using Sen381.Business.Models;
using Sen381.Business.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sen381Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PeersController : ControllerBase
    {
        private readonly IUserService _userService;

        public PeersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchPeers([FromQuery] string? query = null)
        {
            try
            {
                // ✅ Get all users from Supabase
                var users = await _userService.GetAllUsersAsync();

                // ✅ If query is empty or null, return all users (show all peers on page load)
                if (string.IsNullOrWhiteSpace(query))
                {
                    var allPeers = users.Select(u => new
                    {
                        UserId = u.Id,
                        u.FirstName,
                        u.LastName,
                        Role = u.RoleString?.ToLower() ?? "student",
                        u.ProfilePicturePath,
                        u.Program,
                        u.Year,
                        Subjects = u.Interests // Map Interests to Subjects for frontend compatibility
                    });

                    return Ok(allPeers);
                }

                // ✅ Filter users by name when a query is provided
                users = users.Where(u =>
                    u.FirstName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    u.LastName.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // ✅ Map only what's needed for the peers page
                var peers = users.Select(u => new
                {
                    UserId = u.Id,
                    u.FirstName,
                    u.LastName,
                    Role = u.RoleString?.ToLower() ?? "student",
                    u.ProfilePicturePath,
                    u.Program,
                    u.Year,
                    Subjects = u.Interests // Map Interests to Subjects for frontend compatibility
                });

                return Ok(peers);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in SearchPeers: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
