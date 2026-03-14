using Moq;
using psi25_project.Services;
using psi25_project.Models;
using psi25_project.Models.Dtos;
using psi25_project.Repositories.Interfaces;

namespace Geohunt.Tests.Services;

public class GameServiceTests
{
    private readonly Mock<IGameRepository> _mockRepository;
    private readonly GameService _service;

    public GameServiceTests()
    {
        _mockRepository = new Mock<IGameRepository>();
        _service = new GameService(_mockRepository.Object);
    }

    [Fact]
    public async Task StartGameAsync_ValidDto_CreatesGameWithCorrectProperties()
    {
        // Arrange
        var gameDto = new CreateGameDto
        {
            UserId = Guid.NewGuid(),
            TotalRounds = 5
        };

        Game? capturedGame = null;
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Game>()))
            .Callback<Game>(g => capturedGame = g)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.StartGameAsync(gameDto);

        // Assert
        Assert.NotNull(capturedGame);
        Assert.NotEqual(Guid.Empty, capturedGame.Id);
        Assert.Equal(gameDto.UserId, capturedGame.UserId);
        Assert.NotEqual(default(DateTime), capturedGame.StartedAt);
        Assert.Null(capturedGame.FinishedAt);
        Assert.Equal(1, capturedGame.CurrentRound);
        Assert.Equal(0, capturedGame.TotalScore);
        Assert.Equal(gameDto.TotalRounds, capturedGame.TotalRounds);
        _mockRepository.Verify(r => r.AddAsync(capturedGame), Times.Once);
    }

    [Fact]
    public async Task StartGameAsync_ZeroRounds_DefaultsToThreeRounds()
    {
        // Arrange
        var gameDto = new CreateGameDto
        {
            UserId = Guid.NewGuid(),
            TotalRounds = 0
        };

        Game? capturedGame = null;
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Game>()))
            .Callback<Game>(g => capturedGame = g)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.StartGameAsync(gameDto);

        // Assert
        Assert.NotNull(capturedGame);
        Assert.NotEqual(Guid.Empty, capturedGame.Id);
        Assert.Equal(gameDto.UserId, capturedGame.UserId);
        Assert.NotEqual(default(DateTime), capturedGame.StartedAt);
        Assert.Null(capturedGame.FinishedAt);
        Assert.Equal(1, capturedGame.CurrentRound);
        Assert.Equal(0, capturedGame.TotalScore);
        Assert.Equal(3, capturedGame.TotalRounds);
        _mockRepository.Verify(r => r.AddAsync(capturedGame), Times.Once);
    }

    [Fact]
    public async Task StartGameAsync_NegativeRounds_DefaultsToThreeRounds()
    {
        // Arrange
        var gameDto = new CreateGameDto
        {
            UserId = Guid.NewGuid(),
            TotalRounds = -5
        };

        Game? capturedGame = null;
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Game>()))
            .Callback<Game>(g => capturedGame = g)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.StartGameAsync(gameDto);

        // Assert
        Assert.NotNull(capturedGame);
        Assert.NotEqual(Guid.Empty, capturedGame.Id);
        Assert.Equal(gameDto.UserId, capturedGame.UserId);
        Assert.NotEqual(default(DateTime), capturedGame.StartedAt);
        Assert.Null(capturedGame.FinishedAt);
        Assert.Equal(1, capturedGame.CurrentRound);
        Assert.Equal(0, capturedGame.TotalScore);
        Assert.Equal(3, capturedGame.TotalRounds);
        _mockRepository.Verify(r => r.AddAsync(capturedGame), Times.Once);
    }

    [Fact]
    public async Task FinishGameAsync_GameExists_SetsFinishedAtAndReturns()
    {
        // Arrange
        var gameId = Guid.NewGuid();

        var game = new Game
        {
            Id = gameId,
            FinishedAt = null,
            User = null!
        };

        _mockRepository.Setup(r => r.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<Game>()))
            .Returns(Task.CompletedTask);

        var before = DateTime.UtcNow;

        // Act
        var result = await _service.FinishGameAsync(gameId);
        var after = DateTime.UtcNow;

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.FinishedAt);
        Assert.InRange(result.FinishedAt.Value, before, after);
        Assert.NotNull(game.FinishedAt);
        Assert.InRange(game.FinishedAt.Value, before, after);
        _mockRepository.Verify(
            r => r.UpdateAsync(It.Is<Game>(g => g == game && g.FinishedAt != null)),
            Times.Once);
    }

    [Fact]
    public async Task FinishGameAsync_GameNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(gameId))
            .ReturnsAsync((Game?)null);
        
        // Act & Assert
        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _service.FinishGameAsync(gameId)
        );
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Game>()), Times.Never);
    }

    [Fact]
    public async Task FinishGameAsync_GameAlreadyFinished_ThrowsInvalidOperationException()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = new Game
        {
            Id = gameId,
            User = null!,
            FinishedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.GetByIdAsync(gameId))
            .ReturnsAsync(game);
        
        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.FinishGameAsync(gameId)
        );
        _mockRepository.Verify(r => r.UpdateAsync(game), Times.Never);
    }

    [Fact]
    public async Task UpdateScoreAsync_GameExists_UpdatesScore()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var originalScore = 1000;
        var scoreToAdd = 999;
        var game = new Game
        {
            Id = gameId,
            User = null!,
            TotalScore = originalScore
        };

        _mockRepository.Setup(r => r.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        var result = await _service.UpdateScoreAsync(gameId, scoreToAdd);

        // Assert
        Assert.Equal(originalScore + scoreToAdd, result.TotalScore);
        _mockRepository.Verify(r => r.UpdateAsync(It.Is<Game>(g => g.TotalScore == 1999)), Times.Once);
    }

    [Fact]
    public async Task GetTotalScoreAsync_GameExists_ReturnsTotalScore()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = new Game
        {
            Id = gameId,
            User = null!,
            TotalScore = 1000
        };
        _mockRepository.Setup(r => r.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        var result = await _service.GetTotalScoreAsync(gameId);
        
        // Assert
        Assert.Equal(1000, result);
    }
[Fact]
    public async Task GetGameByIdAsync_ExistingGame_ReturnsGameDto()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var startedAt = DateTime.UtcNow;
        var finishedAt = DateTime.UtcNow.AddHours(1);
        
        var game = new Game
        {
            Id = gameId,
            UserId = userId,
            User = null!,
            TotalScore = 1500,
            StartedAt = startedAt,
            FinishedAt = finishedAt,
            CurrentRound = 5,
            TotalRounds = 10
        };

        _mockRepository.Setup(r => r.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        var result = await _service.GetGameByIdAsync(gameId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(gameId, result.Id);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(1500, result.TotalScore);
        Assert.Equal(startedAt, result.StartedAt);
        Assert.Equal(finishedAt, result.FinishedAt);
        Assert.Equal(5, result.CurrentRound);
        Assert.Equal(10, result.TotalRounds);
        
        _mockRepository.Verify(r => r.GetByIdAsync(gameId), Times.Once);
    }

    [Fact]
    public async Task GetGameByIdAsync_NonExistingGame_ReturnsNull()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        
        _mockRepository.Setup(r => r.GetByIdAsync(gameId))
            .ReturnsAsync((Game?)null);

        // Act
        var result = await _service.GetGameByIdAsync(gameId);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(r => r.GetByIdAsync(gameId), Times.Once);
    }

    [Fact]
    public async Task GetGameByIdAsync_GameWithNullFinishedAt_ReturnsDto()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var game = new Game
        {
            Id = gameId,
            UserId = Guid.NewGuid(),
            User = null!,
            TotalScore = 500,
            StartedAt = DateTime.UtcNow,
            FinishedAt = null,
            CurrentRound = 3,
            TotalRounds = 10
        };

        _mockRepository.Setup(r => r.GetByIdAsync(gameId))
            .ReturnsAsync(game);

        // Act
        var result = await _service.GetGameByIdAsync(gameId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(gameId, result.Id);
        Assert.Null(result.FinishedAt);
        Assert.Equal(3, result.CurrentRound);
        
        _mockRepository.Verify(r => r.GetByIdAsync(gameId), Times.Once);
    }
}