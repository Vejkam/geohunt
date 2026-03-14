using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using psi25_project.Models;
using psi25_project.Services.Interfaces;

namespace Geohunt.Tests.Controllers;

public class AchievementControllerTests
{
    private readonly Mock<IAchievementService> _mockService;
    private readonly AchievementController _controller;

    public AchievementControllerTests()
    {
        _mockService = new Mock<IAchievementService>();
        _controller = new AchievementController(_mockService.Object);
    }

    [Fact]
    public async Task GetAllAvailableAchievements_ReturnsOk_WhenAchievementsExist()
    {
        // Arrange
        var achievements = new List<AchievementDto>
        {
            new AchievementDto
            {
                Code = AchievementCodes.FirstGuess,
                Name = "First Guess",
                Description = "Make your first guess",
                UnlockedAt = null
            }
        };
        _mockService.Setup(s => s.GetActiveAchievementsAsync())
            .ReturnsAsync(achievements);

        // Act
        var result = await _controller.GetAllAvailableAchievements();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(achievements, okResult.Value);
    }

    [Fact]
    public async Task GetAllAvailableAchievements_ReturnsNoContent_WhenListIsEmpty()
    {
        // Arrange
        _mockService.Setup(s => s.GetActiveAchievementsAsync())
            .ReturnsAsync(new List<AchievementDto>());

        // Act
        var result = await _controller.GetAllAvailableAchievements();

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result.Result);
    }

    [Fact]
    public async Task GetAllAvailableAchievements_Returns500WithCorrectMessage_WhenServiceThrowsException()
    {
        // Arrange
        var exceptionMessage = "Database connection failed";
        _mockService.Setup(s => s.GetActiveAchievementsAsync())
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _controller.GetAllAvailableAchievements();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Contains("Failed to get available achievements", problemDetails.Detail);
        Assert.Contains(exceptionMessage, problemDetails.Detail);
    }

    [Fact]
    public async Task GetUnlockedAchievementsForUser_ReturnsOk_WhenAchievementsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var achievements = new List<AchievementDto>
        {
            new AchievementDto
            {
                Code = AchievementCodes.FirstGuess,
                Name = "First Guess",
                Description = "Make your first guess",
                UnlockedAt = new DateTime(2025, 12, 05, 18, 23, 44, DateTimeKind.Utc)
            }
        };
        _mockService.Setup(s => s.GetAchievementsForUserAsync(userId))
            .ReturnsAsync(achievements);

        // Act
        var result = await _controller.GetUnlockedAchievementsForUser(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(achievements, okResult.Value);
    }

    [Fact]
    public async Task GetUnlockedAchievementsForUser_ReturnsNoContent_WhenNoAchievementsUnlocked()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockService.Setup(s => s.GetAchievementsForUserAsync(userId))
            .ReturnsAsync(new List<AchievementDto>());

        // Act
        var result = await _controller.GetUnlockedAchievementsForUser(userId);

        // Assert
        var noContentResult = Assert.IsType<NoContentResult>(result.Result);
    }

    [Fact]
    public async Task GetUnlockedAchievementsForUser_Returns500WithCorrectMessage_WhenServiceThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exceptionMessage = "Database connection failed";
        _mockService.Setup(s => s.GetAchievementsForUserAsync(userId))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _controller.GetUnlockedAchievementsForUser(userId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Contains("Failed to get unlocked achievements", problemDetails.Detail);
        Assert.Contains(exceptionMessage, problemDetails.Detail);
    }

    [Fact]
    public async Task GetUserStats_ReturnsOk_WithUsersStats()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userStats = new UserStatsDto
        {
            TotalGames = 21,
            CurrentStreakDays = 4,
            LongestStreakDays = 10
        };

        _mockService.Setup(s => s.GetUserStatsAsync(userId))
            .ReturnsAsync(userStats);

        // Act
        var result = await _controller.GetUserStats(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(userStats, okResult.Value);
        _mockService.Verify(s => s.GetUserStatsAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserStats_ReturnsOk_WhenStatsEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userStats = new UserStatsDto
        {
            TotalGames = 0,
            CurrentStreakDays = 0,
            LongestStreakDays = 0
        };

        _mockService.Setup(s => s.GetUserStatsAsync(userId))
            .ReturnsAsync(userStats);

        // Act
        var result = await _controller.GetUserStats(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(userStats, okResult.Value);
        _mockService.Verify(s => s.GetUserStatsAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserStats_Returns500WithCorrectMessage_WhenServiceThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exceptionMessage = "Database connection failed";
        _mockService.Setup(s => s.GetUserStatsAsync(userId))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _controller.GetUserStats(userId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Contains("Failed to get user stats", problemDetails.Detail);
        Assert.Contains(exceptionMessage, problemDetails.Detail);
    }
}