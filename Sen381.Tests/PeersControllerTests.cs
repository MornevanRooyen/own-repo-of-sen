using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Sen381Backend.Controllers;
using Sen381.Business.Models;
using Sen381.Business.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;

namespace Sen381.Tests
{
    public class PeersControllerTests
    {
        // --- Mock Data ---
        private readonly List<User> mockUsers = new List<User>
        {
            new User { Id = 1, FirstName = "Ella", LastName = "Student", RoleString = "Student" },
            new User { Id = 2, FirstName = "Casey", LastName = "Patel", RoleString = "Student" },
            new User { Id = 3, FirstName = "Elliot", LastName = "Peterson", RoleString = "Student" },
            new User { Id = 4, FirstName = "Morgan", LastName = "Campbell", RoleString = "Student" },
            new User { Id = 5, FirstName = "Taylor", LastName = "Mitchell", RoleString = "Student" },
            new User { Id = 6, FirstName = "Gabriella", LastName = "Petersen", RoleString = "Student" }
        };

        // Should return matching user by first name
        [Fact]
        public async Task SearchPeers_ShouldReturnMatchingUser_WhenQueryMatchesFirstName()
        {
            var mockService = new Mock<IUserService>();
            mockService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(mockUsers);

            var controller = new PeersController(mockService.Object);

            var result = await controller.SearchPeers("Ella") as OkObjectResult;
            var peersJson = JsonSerializer.Serialize(result?.Value);

            Assert.NotNull(result);
            Assert.Contains("Ella", peersJson);
        }

        // Should ignore case in search
        [Fact]
        public async Task SearchPeers_ShouldIgnoreCase_WhenMatchingNames()
        {
            var mockService = new Mock<IUserService>();
            mockService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(mockUsers);

            var controller = new PeersController(mockService.Object);

            var result = await controller.SearchPeers("elliot") as OkObjectResult;
            var peersJson = JsonSerializer.Serialize(result?.Value);

            Assert.NotNull(result);
            Assert.Contains("Elliot", peersJson);
        }

        // Should return multiple matches when query matches several names
        [Fact]
        public async Task SearchPeers_ShouldReturnMultiple_WhenQueryMatchesSeveralNames()
        {
            var mockService = new Mock<IUserService>();
            mockService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(mockUsers);

            var controller = new PeersController(mockService.Object);

            var result = await controller.SearchPeers("El") as OkObjectResult;
            var peersJson = JsonSerializer.Serialize(result?.Value);

            Assert.NotNull(result);
            Assert.Contains("Ella", peersJson);
            Assert.Contains("Elliot", peersJson);
        }

        // Should return empty list when no match found
        [Fact]
        public async Task SearchPeers_ShouldReturnEmptyList_WhenNoMatchesFound()
        {
            var mockService = new Mock<IUserService>();
            mockService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(mockUsers);

            var controller = new PeersController(mockService.Object);

            var result = await controller.SearchPeers("Zach") as OkObjectResult;
            var peersJson = JsonSerializer.Serialize(result?.Value);

            Assert.NotNull(result);
            Assert.DoesNotContain("Zach", peersJson);
        }
    }
}
