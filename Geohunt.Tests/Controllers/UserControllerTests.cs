using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using psi25_project.Models.Dtos;
using psi25_project.Services.Interfaces;

namespace Geohunt.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _mockService;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockService = new Mock<IUserService>();
            _controller = new UserController(_mockService.Object);
        }

        [Fact]
        public async Task GetUsers_ReturnsOkWithUserList()
        {
            // Arrange
            var users = new List<UserAccountDto>
            {
                new UserAccountDto
                {
                    Id = Guid.NewGuid(),
                    Username = "user1",
                    Email = "user1@example.com",
                    CreatedAt = DateTime.UtcNow,
                    Roles = new List<string> { "User" }
                },
                new UserAccountDto
                {
                    Id = Guid.NewGuid(),
                    Username = "user2",
                    Email = "user2@example.com",
                    CreatedAt = DateTime.UtcNow,
                    Roles = new List<string> { "Admin" }
                }
            };

            _mockService.Setup(s => s.GetAllUsersAsync())
                .ReturnsAsync(users);

            // Act
            var result = await _controller.GetUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            var returnedUsers = Assert.IsAssignableFrom<List<UserAccountDto>>(okResult.Value);
            Assert.Equal(2, returnedUsers.Count);
            _mockService.Verify(s => s.GetAllUsersAsync(), Times.Once);
        }

        [Fact]
        public async Task GetUsers_EmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var emptyList = new List<UserAccountDto>();
            _mockService.Setup(s => s.GetAllUsersAsync())
                .ReturnsAsync(emptyList);

            // Act
            var result = await _controller.GetUsers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            var returnedUsers = Assert.IsAssignableFrom<List<UserAccountDto>>(okResult.Value);
            Assert.Empty(returnedUsers);
            _mockService.Verify(s => s.GetAllUsersAsync(), Times.Once);
        }

        [Fact]
        public async Task GetCurrentUser_AuthenticatedUser_ReturnsOk()
        {
            // Arrange
            var userDto = new UserAccountDto
            {
                Id = Guid.NewGuid(),
                Username = "currentuser",
                Email = "current@example.com",
                CreatedAt = DateTime.UtcNow,
                Roles = new List<string> { "User" }
            };

            _mockService.Setup(s => s.GetCurrentUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync(userDto);

            // Act
            var result = await _controller.GetCurrentUser();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            var returnedUser = Assert.IsType<UserAccountDto>(okResult.Value);
            Assert.Equal(userDto.Username, returnedUser.Username);
            _mockService.Verify(s => s.GetCurrentUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()), Times.Once);
        }

        [Fact]
        public async Task GetCurrentUser_ServiceReturnsNull_ReturnsUnauthorized()
        {
            // Arrange
            _mockService.Setup(s => s.GetCurrentUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync((UserAccountDto?)null);

            // Act
            var result = await _controller.GetCurrentUser();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
            Assert.Equal("User is not logged in.", unauthorizedResult.Value);
            _mockService.Verify(s => s.GetCurrentUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()), Times.Once);
        }

        [Fact]
        public async Task GetUser_ExistingUser_ReturnsOk()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userDto = new UserAccountDto
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                CreatedAt = DateTime.UtcNow,
                Roles = new List<string> { "User" }
            };

            _mockService.Setup(s => s.GetUserByIdAsync(userId))
                .ReturnsAsync(userDto);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            var returnedUser = Assert.IsType<UserAccountDto>(okResult.Value);
            Assert.Equal(userId, returnedUser.Id);
            _mockService.Verify(s => s.GetUserByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetUser_NonExistingUser_ReturnsNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockService.Setup(s => s.GetUserByIdAsync(userId))
                .ReturnsAsync((UserAccountDto?)null);

            // Act
            var result = await _controller.GetUser(userId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            Assert.Equal("User not found.", notFoundResult.Value);
            _mockService.Verify(s => s.GetUserByIdAsync(userId), Times.Once);
        }

        [Fact]
        public async Task DeleteUser_Success_ReturnsOkWithMessage()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockService.Setup(s => s.DeleteUserAsync(userId))
                .ReturnsAsync((true, Enumerable.Empty<IdentityError>()));

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            _mockService.Verify(s => s.DeleteUserAsync(userId), Times.Once);
        }

        [Fact]
        public async Task DeleteUser_Failed_ReturnsBadRequestWithErrors()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var errors = new List<IdentityError>
            {
                new IdentityError { Code = "UserNotFound", Description = "User not found" }
            };

            _mockService.Setup(s => s.DeleteUserAsync(userId))
                .ReturnsAsync((false, errors));

            // Act
            var result = await _controller.DeleteUser(userId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            _mockService.Verify(s => s.DeleteUserAsync(userId), Times.Once);
        }
    }
}
