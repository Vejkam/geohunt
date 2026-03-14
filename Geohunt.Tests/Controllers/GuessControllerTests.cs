using Microsoft.AspNetCore.Mvc;
using Moq;
using psi25_project.Models.Dtos;
using psi25_project.Services.Interfaces;

namespace Geohunt.Tests.Controllers;

public class GuessControllerTests
{
    private readonly Mock<IGuessService> _mockGuessService;
    private readonly GuessController _controller;

    public GuessControllerTests()
    {
        _mockGuessService = new Mock<IGuessService>();
        _controller = new GuessController(_mockGuessService.Object);
    }

    [Fact]
    public async Task CreateGuess_ValidDto_ReturnsCreatedWithGuessData()
    {
        // Arrange
        var dto = new CreateGuessDto {};
        var guessDto = new GuessResponseDto { Id = 1 };
        var finished = false;
        var currentRound = 3;
        var totalScore = 1500;
        var newAchievements = new List<AchievementUnlockDto>();

        _mockGuessService
            .Setup(s => s.CreateGuessAsync(dto))
            .ReturnsAsync((guessDto, finished, currentRound, totalScore, (IReadOnlyList<AchievementUnlockDto>)newAchievements));

        // Act
        var result = await _controller.CreateGuess(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);

        _mockGuessService.Verify(s => s.CreateGuessAsync(dto), Times.Once);
    }

    [Fact]
    public async Task CreateGuess_GameFinished_ReturnsCreatedWithFinishedTrue()
    {
        // Arrange
        var dto = new CreateGuessDto { /* properties */ };
        var guessDto = new GuessResponseDto { Id = 1 };
        var finished = true;
        var currentRound = 5;
        var totalScore = 5000;
        var newAchievements = new List<AchievementUnlockDto>();


        _mockGuessService
            .Setup(s => s.CreateGuessAsync(dto))
            .ReturnsAsync((guessDto, finished, currentRound, totalScore, (IReadOnlyList<AchievementUnlockDto>)newAchievements));

        // Act
        var result = await _controller.CreateGuess(dto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
    }

    [Fact]
    public async Task CreateGuess_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("GameId", "GameId is required");

        // Act
        var result = await _controller.CreateGuess(new CreateGuessDto());

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
        
        _mockGuessService.Verify(s => s.CreateGuessAsync(It.IsAny<CreateGuessDto>()), Times.Never);
    }

    [Fact]
    public async Task CreateGuess_GameNotFound_ReturnsNotFound()
    {
        // Arrange
        var dto = new CreateGuessDto {};
        var errorMessage = "Game not found";

        _mockGuessService
            .Setup(s => s.CreateGuessAsync(dto))
            .ThrowsAsync(new KeyNotFoundException(errorMessage));

        // Act
        var result = await _controller.CreateGuess(dto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
        Assert.Equal(errorMessage, notFoundResult.Value);
    }

    [Fact]
    public async Task CreateGuess_InvalidOperation_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateGuessDto {};
        var errorMessage = "Game is already finished";

        _mockGuessService
            .Setup(s => s.CreateGuessAsync(dto))
            .ThrowsAsync(new InvalidOperationException(errorMessage));

        // Act
        var result = await _controller.CreateGuess(dto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
        Assert.Equal(errorMessage, badRequestResult.Value);
    }

    [Fact]
    public async Task GetGuessesForGame_ExistingGame_ReturnsOkWithGuesses()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var guesses = new List<GuessResponseDto>
        {
            new GuessResponseDto { Id = 1, GameId = gameId },
            new GuessResponseDto { Id = 2, GameId = gameId },
            new GuessResponseDto { Id = 3, GameId = gameId }
        };

        _mockGuessService
            .Setup(s => s.GetGuessesForGameAsync(gameId))
            .ReturnsAsync(guesses);

        // Act
        var result = await _controller.GetGuessesForGame(gameId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        
        var returnedGuesses = Assert.IsAssignableFrom<IEnumerable<GuessResponseDto>>(okResult.Value);
        Assert.Equal(3, returnedGuesses.Count());
        
        _mockGuessService.Verify(s => s.GetGuessesForGameAsync(gameId), Times.Once);
    }

    [Fact]
    public async Task GetGuessesForGame_NoGuesses_ReturnsOkWithEmptyList()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var emptyGuesses = new List<GuessResponseDto>();

        _mockGuessService
            .Setup(s => s.GetGuessesForGameAsync(gameId))
            .ReturnsAsync(emptyGuesses);

        // Act
        var result = await _controller.GetGuessesForGame(gameId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedGuesses = Assert.IsAssignableFrom<IEnumerable<GuessResponseDto>>(okResult.Value);
        Assert.Empty(returnedGuesses);
    }
}