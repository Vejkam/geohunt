using Microsoft.AspNetCore.Mvc;
using Moq;
using psi25_project.Controllers;
using psi25_project.Models.Dtos;
using psi25_project.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace Geohunt.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<IAccountService> _mockService;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _mockService = new Mock<IAccountService>();
            _controller = new AccountController(_mockService.Object);
        }

        [Fact]
        public async Task Register_ValidModel_ReturnsOk()
        {
            // Arrange
            var dto = new RegisterDto { Username = "user", Email = "a@b.com", Password = "Password123!" };

            _mockService.Setup(s => s.RegisterAsync(It.IsAny<RegisterDto>()))
                .ReturnsAsync((true, null as IEnumerable<IdentityError>));

            // Act
            var result = await _controller.Register(dto);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
        }

        [Fact]
        public async Task Register_ServiceFails_ReturnsBadRequest()
        {
            // Arrange
            var dto = new RegisterDto { Username = "user", Email = "a@b.com", Password = "Password123!" };
            var errors = new List<IdentityError> { new IdentityError { Description = "Failed" } };

            _mockService.Setup(s => s.RegisterAsync(It.IsAny<RegisterDto>()))
                .ReturnsAsync((false, errors));

            // Act
            var result = await _controller.Register(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequest.StatusCode);
        }

        [Fact]
        public async Task Login_Valid_ReturnsOk()
        {
            // Arrange
            var dto = new LoginDto { Username = "user", Password = "Password123!" };

            _mockService.Setup(s => s.LoginAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync((true, null as string));

            // Act
            var result = await _controller.Login(dto);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
        }

        [Fact]
        public async Task Login_Invalid_ReturnsUnauthorized()
        {
            // Arrange
            var dto = new LoginDto { Username = "user", Password = "wrong" };

            _mockService.Setup(s => s.LoginAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync((false, "Invalid username or password."));

            // Act
            var result = await _controller.Login(dto);

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorized.StatusCode);
        }

        [Fact]
        public async Task Logout_ReturnsOk()
        {
            // Arrange
            _mockService.Setup(s => s.LogoutAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Logout();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
        }

        [Fact]
        public async Task AssignAdmin_Success_ReturnsOk()
        {
            // Arrange
            var dto = new AssignRoleDto { UserId = Guid.NewGuid() };

            _mockService.Setup(s => s.AssignAdminAsync(It.IsAny<Guid>()))
                .ReturnsAsync((true, null as IEnumerable<string>));

            // Act
            var result = await _controller.AssignAdmin(dto);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, ok.StatusCode);
        }

        [Fact]
        public async Task AssignAdmin_Failure_ReturnsBadRequest()
        {
            // Arrange
            var dto = new AssignRoleDto { UserId = Guid.NewGuid() };
            var errors = new List<string> { "User not found" };

            _mockService.Setup(s => s.AssignAdminAsync(It.IsAny<Guid>()))
                .ReturnsAsync((false, errors));

            // Act
            var result = await _controller.AssignAdmin(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequest.StatusCode);
        }
    }
}
