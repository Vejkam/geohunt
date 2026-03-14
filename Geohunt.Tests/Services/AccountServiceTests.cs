using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using psi25_project.Models;
using psi25_project.Models.Dtos;
using psi25_project.Services;

namespace Geohunt.Tests.Services;
public class AccountServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly AccountService _service;

    public AccountServiceTests()
    {
        _userManagerMock = CreateUserManagerMock();
        _signInManagerMock = CreateSignInManagerMock(_userManagerMock.Object);

        _service = new AccountService(_userManagerMock.Object, _signInManagerMock.Object);
    }

    private Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();

        var options = new Mock<IOptions<IdentityOptions>>();
        options.Setup(o => o.Value).Returns(new IdentityOptions());

        var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
        var userValidators = Array.Empty<IUserValidator<ApplicationUser>>();
        var passwordValidators = Array.Empty<IPasswordValidator<ApplicationUser>>();

        var keyNormalizer = new Mock<ILookupNormalizer>();
        var errors = new IdentityErrorDescriber();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();

        var userManager = new Mock<UserManager<ApplicationUser>>(
            store.Object,
            options.Object,
            passwordHasher.Object,
            userValidators,
            passwordValidators,
            keyNormalizer.Object,
            errors,
            services.Object,
            logger.Object
        );

        return userManager;
    }

    private Mock<SignInManager<ApplicationUser>> CreateSignInManagerMock(UserManager<ApplicationUser> userManager)
    {
        var contextAccessor = new Mock<IHttpContextAccessor>();
        contextAccessor.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());

        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();

        var options = new Mock<IOptions<IdentityOptions>>();
        options.Setup(o => o.Value).Returns(new IdentityOptions());

        var logger = new Mock<ILogger<SignInManager<ApplicationUser>>>();

        var schemes = new Mock<IAuthenticationSchemeProvider>();

        var userConfirmation = new Mock<IUserConfirmation<ApplicationUser>>();
        userConfirmation
            .Setup(c => c.IsConfirmedAsync(userManager, It.IsAny<ApplicationUser>()))
            .ReturnsAsync(true);

        var signInManager = new Mock<SignInManager<ApplicationUser>>(
            userManager,
            contextAccessor.Object,
            claimsFactory.Object,
            options.Object,
            logger.Object,
            schemes.Object,
            userConfirmation.Object
        );

        return signInManager;
    }

        [Fact]
        public async Task RegisterAsync_WithValidModel_ShouldReturnSuccessAndAddPlayerRole()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Test@123"
            };

            ApplicationUser? capturedUser = null;

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, string>((user, password) => capturedUser = user);

            _userManagerMock
                .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Player"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.RegisterAsync(registerDto);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Null(result.Errors);
            
            Assert.NotNull(capturedUser);
            Assert.Equal(registerDto.Username, capturedUser.UserName);
            Assert.Equal(registerDto.Email, capturedUser.Email);
            Assert.True(capturedUser.CreatedAt <= DateTime.UtcNow);
            Assert.True(capturedUser.CreatedAt > DateTime.UtcNow.AddSeconds(-5)); // Within last 5 seconds

            _userManagerMock.Verify(x => x.CreateAsync(
                It.Is<ApplicationUser>(u => u.UserName == registerDto.Username && u.Email == registerDto.Email),
                registerDto.Password), Times.Once);

            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Player"), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WhenUserCreationFails_ShouldReturnFailureWithErrors()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Test@123"
            };

            var identityErrors = new[]
            {
                new IdentityError { Code = "DuplicateUserName", Description = "Username already exists" },
                new IdentityError { Code = "InvalidEmail", Description = "Email is invalid" }
            };

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(identityErrors));

            // Act
            var result = await _service.RegisterAsync(registerDto);

            // Assert
            Assert.False(result.Succeeded);
            Assert.NotNull(result.Errors);
            Assert.Equal(2, result.Errors.Count());
            Assert.Contains(result.Errors, e => e.Code == "DuplicateUserName");
            Assert.Contains(result.Errors, e => e.Code == "InvalidEmail");

            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_ShouldSetCreatedAtToUtcNow()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "Test@123"
            };

            var beforeCall = DateTime.UtcNow;
            ApplicationUser? capturedUser = null;

            _userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, string>((user, password) => capturedUser = user);

            _userManagerMock
                .Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "Player"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await _service.RegisterAsync(registerDto);
            var afterCall = DateTime.UtcNow;

            // Assert
            Assert.NotNull(capturedUser);
            Assert.True(capturedUser.CreatedAt >= beforeCall);
            Assert.True(capturedUser.CreatedAt <= afterCall);
            Assert.Equal(DateTimeKind.Utc, capturedUser.CreatedAt.Kind);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccess()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Username = "testuser",
                Password = "Test@123"
            };

            _signInManagerMock
                .Setup(x => x.PasswordSignInAsync(loginDto.Username, loginDto.Password, false, false))
                .ReturnsAsync(SignInResult.Success);

            // Act
            var result = await _service.LoginAsync(loginDto);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Null(result.Error);

            _signInManagerMock.Verify(x => x.PasswordSignInAsync(
                loginDto.Username, 
                loginDto.Password, 
                false, 
                false), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ShouldReturnFailureWithErrorMessage()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Username = "testuser",
                Password = "WrongPassword"
            };

            _signInManagerMock
                .Setup(x => x.PasswordSignInAsync(loginDto.Username, loginDto.Password, false, false))
                .ReturnsAsync(SignInResult.Failed);

            // Act
            var result = await _service.LoginAsync(loginDto);

            // Assert
            Assert.False(result.Succeeded);
            Assert.NotNull(result.Error);
            Assert.Equal("Invalid username or password.", result.Error);

            _signInManagerMock.Verify(x => x.PasswordSignInAsync(
                loginDto.Username, 
                loginDto.Password, 
                false, 
                false), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldNotSetPersistentCookie()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Username = "testuser",
                Password = "Test@123"
            };

            _signInManagerMock
                .Setup(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Success);

            // Act
            await _service.LoginAsync(loginDto);

            // Assert
            _signInManagerMock.Verify(x => x.PasswordSignInAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                false,
                It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldNotEnableLockoutOnFailure()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Username = "testuser",
                Password = "Test@123"
            };

            _signInManagerMock
                .Setup(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Success);

            // Act
            await _service.LoginAsync(loginDto);

            // Assert
            _signInManagerMock.Verify(x => x.PasswordSignInAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<bool>(),
                false), Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_ShouldCallSignOutAsync()
        {
            // Arrange
            _signInManagerMock
                .Setup(x => x.SignOutAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _service.LogoutAsync();

            // Assert
            _signInManagerMock.Verify(x => x.SignOutAsync(), Times.Once);
        }

        [Fact]
        public async Task AssignAdminAsync_WhenUserNotFound_ShouldReturnFailureWithError()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _service.AssignAdminAsync(userId);

            // Assert
            Assert.False(result.Succeeded);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Contains("User not found", result.Errors);

            _userManagerMock.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
            _userManagerMock.Verify(x => x.IsInRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AssignAdminAsync_WhenUserExists_ShouldAddAdminRole()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@example.com"
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.IsInRoleAsync(user, "Admin"))
                .ReturnsAsync(false);

            _userManagerMock
                .Setup(x => x.AddToRoleAsync(user, "Admin"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.AssignAdminAsync(userId);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Null(result.Errors);

            _userManagerMock.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
            _userManagerMock.Verify(x => x.IsInRoleAsync(user, "Admin"), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(user, "Admin"), Times.Once);
        }

        [Fact]
        public async Task AssignAdminAsync_WhenUserAlreadyAdmin_ShouldNotAddRoleAgain()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser
            {
                Id = userId,
                UserName = "adminuser",
                Email = "admin@example.com"
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.IsInRoleAsync(user, "Admin"))
                .ReturnsAsync(true);

            // Act
            var result = await _service.AssignAdminAsync(userId);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Null(result.Errors);

            _userManagerMock.Verify(x => x.FindByIdAsync(userId.ToString()), Times.Once);
            _userManagerMock.Verify(x => x.IsInRoleAsync(user, "Admin"), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AssignAdminAsync_WithValidUserId_ShouldReturnSuccess()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new ApplicationUser
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@example.com"
            };

            _userManagerMock
                .Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync(user);

            _userManagerMock
                .Setup(x => x.IsInRoleAsync(user, "Admin"))
                .ReturnsAsync(false);

            _userManagerMock
                .Setup(x => x.AddToRoleAsync(user, "Admin"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.AssignAdminAsync(userId);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Null(result.Errors);
        }
}
