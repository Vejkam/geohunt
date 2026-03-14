using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Moq;
using psi25_project.Models;
using psi25_project.Models.Dtos;
using psi25_project.Services.Interfaces;

namespace Geohunt.Tests.Controllers;

public class GameControllerTests
{
    private readonly Mock<IGameService> _mockService;
    private readonly GameController _controller;

    public GameControllerTests()
    {
        _mockService = new Mock<IGameService>();
        _controller = new GameController(_mockService.Object);
    }

    [Fact]
    public async Task StartGame_WithValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateGameDto
        {
            UserId = Guid.NewGuid(),
            TotalRounds = 5
        };

        var responseDto = new GameResponseDto
        {
            Id = Guid.NewGuid(),
            UserId = createDto.UserId,
            TotalScore = 0,
            StartedAt = DateTime.UtcNow,
            FinishedAt = null,
            TotalRounds = 5,
            CurrentRound = 1
        };
        _mockService.Setup(s => s.StartGameAsync(createDto))
            .ReturnsAsync(responseDto);

        // Act
        var result = await _controller.StartGame(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(nameof(_controller.GetGameById), createdResult.ActionName);

        _mockService.Verify(s => s.StartGameAsync(createDto), Times.Once);
    } 

    [Fact]
    public async Task FinishGame_GameExists_ReturnsOk() 
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var gameResult = new Game
        {
            Id = gameId,
            User = null!,
            FinishedAt = null
        };

        _mockService.Setup(s => s.FinishGameAsync(gameId))
            .ReturnsAsync(gameResult);

        // Act
        var result = await _controller.FinishGame(gameId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);

        _mockService.Verify(s => s.FinishGameAsync(gameId), Times.Once);
    }

    [Fact]
    public async Task FinishGame_GameNotFound_ReturnsNotFound() 
    {
        // Arrange
        var gameId = Guid.NewGuid();
        _mockService.Setup(s => s.FinishGameAsync(gameId))
            .ThrowsAsync(new KeyNotFoundException());

        // Act
        var result = await _controller.FinishGame(gameId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
        Assert.Equal("Game not found", notFoundResult.Value);
    }
    [Fact]
    public async Task FinishGame_GameAlreadyFinished_ReturnsBadRequest()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        _mockService.Setup(s => s.FinishGameAsync(gameId))
            .ThrowsAsync(new InvalidOperationException());

        // Act
        var result = await _controller.FinishGame(gameId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }


    [Fact]
    public async Task UpdateScore_GameExists_ReturnsOkWithUpdatedGame()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var updatedGame = new Game
        {
            Id = gameId,
            User = null!,
            TotalScore = 42
        };

        _mockService.Setup(s => s.UpdateScoreAsync(gameId, 42))
            .ReturnsAsync(updatedGame);

        // Act
        var result = await _controller.UpdateScore(gameId, 42);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, ok.StatusCode);
        var returnedGame = Assert.IsType<Game>(ok.Value);

        _mockService.Verify(s => s.UpdateScoreAsync(gameId, 42), Times.Once);
    }

    [Fact]
    public async Task UpdateScore_GameNotFound_ReturnsNotFound()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var score = 100;
        
        _mockService.Setup(s => s.UpdateScoreAsync(gameId, score))
            .ThrowsAsync(new KeyNotFoundException());

        // Act
        var result = await _controller.UpdateScore(gameId, score);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
        Assert.Equal("Game not found", notFoundResult.Value);
        _mockService.Verify(s => s.UpdateScoreAsync(gameId, score), Times.Once);
    }

    [Fact]
    public async Task GetTotalScore_GameExists_ReturnsOk()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var score = 1000;

        _mockService.Setup(s => s.GetTotalScoreAsync(gameId))
            .ReturnsAsync(score);
        
        // Act
        var result = await _controller.GetTotalScore(gameId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(200, okResult.StatusCode);
        _mockService.Verify(s => s.GetTotalScoreAsync(gameId), Times.Once);
    }

    [Fact]
    public async Task GetTotalScore_GameNotFound_ReturnsNotFound()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        
        _mockService.Setup(s => s.GetTotalScoreAsync(gameId))
            .ThrowsAsync(new KeyNotFoundException());

        // Act
        var result = await _controller.GetTotalScore(gameId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
        Assert.Equal("Game not found", notFoundResult.Value);
        _mockService.Verify(s => s.GetTotalScoreAsync(gameId), Times.Once);
    }

    [Fact]
    public async Task GetGameById_GameExists_ReturnsOk()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var gameDto = new GameResponseDto
        {
            Id = gameId
        };

        _mockService.Setup(s => s.GetGameByIdAsync(gameId))
            .ReturnsAsync(gameDto);

        // Act
        var result = await _controller.GetGameById(gameId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDto = Assert.IsType<GameResponseDto>(okResult.Value);

        Assert.Equal(gameId, returnedDto.Id);
        _mockService.Verify(s => s.GetGameByIdAsync(gameId), Times.Once);
    }

    [Fact]
    public async Task GetGameById_GameDoesntExist_ReturnsNotFound()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        
        _mockService.Setup(s => s.GetGameByIdAsync(gameId))
            .ReturnsAsync((GameResponseDto?)null);

        // Act
        var result = await _controller.GetGameById(gameId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
        Assert.Equal("Game not found", notFoundResult.Value);

        _mockService.Verify(s => s.GetGameByIdAsync(gameId), Times.Once);
    }
}