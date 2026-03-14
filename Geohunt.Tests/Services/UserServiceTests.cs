using Microsoft.AspNetCore.Identity;
using Moq;
using psi25_project.Models;
using psi25_project.Repositories;
using psi25_project.Services;
using System.Security.Claims;

namespace Geohunt.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<IUserRepository> _mockRepository;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _mockUserManager = CreateMockUserManager();
            _mockRepository = new Mock<IUserRepository>();
            _service = new UserService(_mockUserManager.Object, _mockRepository.Object);
        }

        private Mock<UserManager<ApplicationUser>> CreateMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        [Fact]
        public async Task GetAllUsersAsync_MultipleUsers_ReturnsUserDtosWithRoles()
        {
            // Arrange
            var user1 = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "user1",
                Email = "user1@example.com",
                CreatedAt = DateTime.UtcNow
            };

            var user2 = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "user2",
                Email = "user2@example.com",
                CreatedAt = DateTime.UtcNow
            };

            var users = new List<ApplicationUser> { user1, user2 };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(users);

            _mockUserManager.Setup(um => um.GetRolesAsync(user1))
                .ReturnsAsync(new List<string> { "User" });

            _mockUserManager.Setup(um => um.GetRolesAsync(user2))
                .ReturnsAsync(new List<string> { "Admin" });

            // Act
            var result = await _service.GetAllUsersAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("user1", result[0].Username);
            Assert.Equal("user2", result[1].Username);
            Assert.Contains("User", result[0].Roles);
            Assert.Contains("Admin", result[1].Roles);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
            _mockUserManager.Verify(um => um.GetRolesAsync(user1), Times.Once);
            _mockUserManager.Verify(um => um.GetRolesAsync(user2), Times.Once);
        }

        [Fact]
        public async Task GetAllUsersAsync_EmptyDatabase_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<ApplicationUser>());

            // Act
            var result = await _service.GetAllUsersAsync();

            // Assert
            Assert.Empty(result);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllUsersAsync_UserWithNoRoles_ReturnsEmptyRolesList()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "user",
                Email = "user@example.com",
                CreatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<ApplicationUser> { user });

            _mockUserManager.Setup(um => um.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            // Act
            var result = await _service.GetAllUsersAsync();

            // Assert
            Assert.Single(result);
            Assert.Empty(result[0].Roles);
            _mockUserManager.Verify(um => um.GetRolesAsync(user), Times.Once);
        }

        [Fact]
        public async Task GetAllUsersAsync_UserWithMultipleRoles_ReturnsAllRoles()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "admin",
                Email = "admin@example.com",
                CreatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<ApplicationUser> { user });

            _mockUserManager.Setup(um => um.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User", "Admin", "Moderator" });

            // Act
            var result = await _service.GetAllUsersAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal(3, result[0].Roles.Count);
            Assert.Contains("User", result[0].Roles);
            Assert.Contains("Admin", result[0].Roles);
            Assert.Contains("Moderator", result[0].Roles);
        }

        [Fact]
        public async Task GetUserByIdAsync_ExistingUser_ReturnsUserDtoWithRoles()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@example.com",
                CreatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(user);

            _mockUserManager.Setup(um => um.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _service.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
            Assert.Equal("testuser", result.Username);
            Assert.Equal("test@example.com", result.Email);
            Assert.Contains("User", result.Roles);
            _mockRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
            _mockUserManager.Verify(um => um.GetRolesAsync(user), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_NonExistingUser_ReturnsNull()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockRepository.Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _service.GetUserByIdAsync(userId);

            // Assert
            Assert.Null(result);
            _mockRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
            _mockUserManager.Verify(um => um.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task GetUserByIdAsync_UserWithMultipleRoles_ReturnsAllRoles()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser
            {
                Id = userId,
                UserName = "poweruser",
                Email = "power@example.com",
                CreatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(user);

            _mockUserManager.Setup(um => um.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User", "Admin" });

            // Act
            var result = await _service.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Roles.Count);
            Assert.Contains("User", result.Roles);
            Assert.Contains("Admin", result.Roles);
        }

        [Fact]
        public async Task GetUserByIdAsync_NullEmail_MapsCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser
            {
                Id = userId,
                UserName = "nomail",
                Email = null,
                CreatedAt = DateTime.UtcNow
            };

            _mockRepository.Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(user);

            _mockUserManager.Setup(um => um.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            // Act
            var result = await _service.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Email);
            Assert.Equal("nomail", result.Username);
        }

        [Fact]
        public async Task GetCurrentUserAsync_ValidPrincipal_ReturnsUserDto()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "currentuser",
                Email = "current@example.com",
                CreatedAt = DateTime.UtcNow
            };

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "currentuser")
            });
            var principal = new ClaimsPrincipal(identity);

            _mockUserManager.Setup(um => um.FindByNameAsync("currentuser"))
                .ReturnsAsync(user);

            _mockUserManager.Setup(um => um.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            // Act
            var result = await _service.GetCurrentUserAsync(principal);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("currentuser", result.Username);
            _mockUserManager.Verify(um => um.FindByNameAsync("currentuser"), Times.Once);
            _mockUserManager.Verify(um => um.GetRolesAsync(user), Times.Once);
        }

        [Fact]
        public async Task GetCurrentUserAsync_NullIdentityName_ReturnsNull()
        {
            // Arrange
            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = await _service.GetCurrentUserAsync(principal);

            // Assert
            Assert.Null(result);
            _mockUserManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetCurrentUserAsync_EmptyIdentityName_ReturnsNull()
        {
            // Arrange
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "")
            });
            var principal = new ClaimsPrincipal(identity);

            // Act
            var result = await _service.GetCurrentUserAsync(principal);

            // Assert
            Assert.Null(result);
            _mockUserManager.Verify(um => um.FindByNameAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetCurrentUserAsync_UserNotFound_ReturnsNull()
        {
            // Arrange
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "nonexistent")
            });
            var principal = new ClaimsPrincipal(identity);

            _mockUserManager.Setup(um => um.FindByNameAsync("nonexistent"))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _service.GetCurrentUserAsync(principal);

            // Assert
            Assert.Null(result);
            _mockUserManager.Verify(um => um.FindByNameAsync("nonexistent"), Times.Once);
            _mockUserManager.Verify(um => um.GetRolesAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }

        [Fact]
        public async Task DeleteUserAsync_ExistingUser_ReturnsSuccessTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser
            {
                Id = userId,
                UserName = "tobedeleted",
                Email = "delete@example.com"
            };

            _mockUserManager.Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _mockUserManager.Setup(um => um.DeleteAsync(user))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var (succeeded, errors) = await _service.DeleteUserAsync(userId);

            // Assert
            Assert.True(succeeded);
            Assert.NotNull(errors);
            Assert.Empty(errors);
            _mockUserManager.Verify(um => um.FindByIdAsync(userId.ToString()), Times.Once);
            _mockUserManager.Verify(um => um.DeleteAsync(user), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_NonExistingUser_ReturnsFailureWithError()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockUserManager.Setup(um => um.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var (succeeded, errors) = await _service.DeleteUserAsync(userId);

            // Assert
            Assert.False(succeeded);
            Assert.NotNull(errors);
            Assert.Single(errors);
            Assert.Equal("User not found", errors.First().Description);
            _mockUserManager.Verify(um => um.FindByIdAsync(userId.ToString()), Times.Once);
            _mockUserManager.Verify(um => um.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Never);
        }
    }
}
