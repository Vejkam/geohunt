using Moq;
using psi25_project.Models;
using psi25_project.Models.Dtos;
using psi25_project.Repositories.Interfaces;
using psi25_project.Services;
using psi25_project.Services.Interfaces;

namespace Geohunt.Tests.Services;

public class GuessServiceTests
{
    private readonly Mock<IGuessRepository> _mockGuessRepository;
    private readonly GuessService _service;
    private readonly Mock<IGameRepository> _mockGameRepository;
    private readonly Mock<ILocationRepository> _mockLocationRepository;
    private readonly Mock<IAchievementService> _achievementService;

    public GuessServiceTests()
    {
        _mockGuessRepository = new Mock<IGuessRepository>();
        _mockGameRepository = new Mock<IGameRepository>();
        _mockLocationRepository = new Mock<ILocationRepository>();
        _achievementService = new Mock<IAchievementService>();
        _service = new GuessService(_mockGuessRepository.Object, _mockGameRepository.Object, _mockLocationRepository.Object, _achievementService.Object);
    }

    [Fact]
    public async Task CreateGuessAsync_MidGame_CreatesGuess_UpdatesGame_AndReturnsExpectedTuple()
    {
        // Arrange
        var dto = new CreateGuessDto
        {
            GameId = Guid.NewGuid(),
            LocationId = 1,
            GuessedLatitude = 10,
            GuessedLongitude = 20,
            DistanceKm = 5,
            Score = 100
        };

        var game = new Game
        {
            Id = dto.GameId,
            UserId = Guid.NewGuid(),
            User = null!,
            FinishedAt = null,
            CurrentRound = 1,
            TotalRounds = 3,
            TotalScore = 0
        };

        var location = new Location
        {
            Id = dto.LocationId,
            Latitude = 50,
            Longitude = 60
        };

        _mockGameRepository.Setup(r => r.GetByIdAsync(dto.GameId))
            .ReturnsAsync(game);

        _mockLocationRepository.Setup(r => r.GetByIdAsync(dto.LocationId))
            .ReturnsAsync(location);

        Guess? capturedGuess = null;

        _mockGuessRepository.Setup(r => r.AddAsync(It.IsAny<Guess>()))
            .Callback<Guess>(g => capturedGuess = g)
            .Returns(Task.CompletedTask);

        _mockGameRepository.Setup(r => r.UpdateAsync(It.IsAny<Game>()))
            .Returns(Task.CompletedTask);

        _achievementService
            .Setup(s => s.OnRoundSubmittedAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<double>(),
                It.IsAny<int>()))
            .ReturnsAsync(new List<UserAchievement>());

        // Act
        var (guessDto, finished, currentRound, totalScore, unlockedAchievements) = await _service.CreateGuessAsync(dto);

        // Assert – guess entity
        Assert.NotNull(capturedGuess);
        Assert.Equal(game.Id, capturedGuess!.GameId);
        Assert.Equal(location.Id, capturedGuess.LocationId);
        Assert.Equal(dto.GuessedLatitude, capturedGuess.GuessedLatitude);
        Assert.Equal(dto.GuessedLongitude, capturedGuess.GuessedLongitude);
        Assert.Equal(dto.DistanceKm, capturedGuess.DistanceKm);
        Assert.Equal(dto.Score, capturedGuess.Score);

        // Assert
        Assert.Equal(2, game.CurrentRound);    
        Assert.Equal(100, game.TotalScore);    
        Assert.Null(game.FinishedAt);

        // Assert
        Assert.False(finished);
        Assert.Equal(game.CurrentRound, currentRound);
        Assert.Equal(game.TotalScore, totalScore);

        Assert.Equal(capturedGuess.Id, guessDto.Id);
        Assert.Equal(capturedGuess.GameId, guessDto.GameId);
        Assert.Equal(capturedGuess.LocationId, guessDto.LocationId);
        Assert.Equal(location.Latitude, guessDto.ActualLatitude);
        Assert.Equal(location.Longitude, guessDto.ActualLongitude);

        _mockGuessRepository.Verify(r => r.AddAsync(It.IsAny<Guess>()), Times.Once);
        _mockGameRepository.Verify(r => r.UpdateAsync(game), Times.Once);

        _achievementService.Verify(
            s => s.OnRoundSubmittedAsync(
                game.UserId,
                game.Id,
                1,
                dto.DistanceKm,
                dto.Score),
            Times.Once);
    }

    [Fact]
    public async Task CreateGuessAsync_GameAlreadyFinished_ThrowsOperationException()
    {
        // Arrange
        var dto = new CreateGuessDto
        {
            GameId = Guid.NewGuid(),
            LocationId = 1,
            GuessedLatitude = 10,
            GuessedLongitude = 20,
            DistanceKm = 5,
            Score = 100
        };

        var game = new Game
        {
            Id = dto.GameId,
            UserId = Guid.NewGuid(),
            User = null!,
            FinishedAt = DateTime.UtcNow.AddHours(-1),
            CurrentRound = 1,
            TotalRounds = 3,
            TotalScore = 0
        };

        _mockGameRepository
            .Setup(r => r.GetByIdAsync(dto.GameId))
            .ReturnsAsync(game);
        
        // Act & assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateGuessAsync(dto)
        );

        Assert.Equal("Game is already finished", exception.Message);

        _mockLocationRepository.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
        _mockGuessRepository.Verify(r => r.AddAsync(It.IsAny<Guess>()), Times.Never);
    }

    [Fact]
    public async Task GetGuessesForGameAsync_ReturnsListOfGuesses()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        
        var location1 = new Location { Id = 1, Latitude = 50, Longitude = 60 };
        var location2 = new Location { Id = 2, Latitude = 40, Longitude = 50 };

        var guesses = new List<Guess>
        {
            new Guess
            {
                Id = 1,
                GameId = gameId,
                Game = null!,
                LocationId = 1,
                Location = location1,
                RoundNumber = 1,
                GuessedLatitude = 51,
                GuessedLongitude = 61,
                DistanceKm = 10,
                Score = 900
            },
            new Guess
            {
                Id = 1,
                GameId = gameId,
                Game = null!,
                LocationId = 2,
                Location = location2,
                RoundNumber = 2,
                GuessedLatitude = 41,
                GuessedLongitude = 51,
                DistanceKm = 15,
                Score = 850
            }
        };

        _mockGuessRepository.Setup(r => r.GetGuessesByGameAsync(gameId))
            .ReturnsAsync(guesses);

        // Act
        var result = await _service.GetGuessesForGameAsync(gameId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Equal(guesses[0].Id, result[0].Id);
        Assert.Equal(guesses[0].GameId, result[0].GameId);
        Assert.Equal(guesses[0].LocationId, result[0].LocationId);
        Assert.Equal(guesses[0].RoundNumber, result[0].RoundNumber);
        Assert.Equal(guesses[0].GuessedLatitude, result[0].GuessedLatitude);
        Assert.Equal(guesses[0].GuessedLongitude, result[0].GuessedLongitude);
        Assert.Equal(location1.Latitude, result[0].ActualLatitude);
        Assert.Equal(location1.Longitude, result[0].ActualLongitude);

        Assert.Equal(guesses[1].Id, result[1].Id);
        Assert.Equal(location2.Latitude, result[1].ActualLatitude);
        Assert.Equal(location2.Longitude, result[1].ActualLongitude);

        _mockGuessRepository.Verify(r => r.GetGuessesByGameAsync(gameId), Times.Once);
    }

    [Fact]
    public async Task CreateGuessAsync_LastRound_FinishesGame_TriggersGameFinishedAchievements()
    {
        // Arrange
        var dto = new CreateGuessDto
        {
            GameId = Guid.NewGuid(),
            LocationId = 1,
            GuessedLatitude = 10,
            GuessedLongitude = 20,
            DistanceKm = 5,
            Score = 100
        };

        var game = new Game
        {
            Id = dto.GameId,
            UserId = Guid.NewGuid(),
            User = null!,
            FinishedAt = null,
            CurrentRound = 3, 
            TotalRounds = 3,
            TotalScore = 200
        };

        var location = new Location
        {
            Id = dto.LocationId,
            Latitude = 50,
            Longitude = 60
        };

        _mockGameRepository.Setup(r => r.GetByIdAsync(dto.GameId))
            .ReturnsAsync(game);

        _mockLocationRepository.Setup(r => r.GetByIdAsync(dto.LocationId))
            .ReturnsAsync(location);

        _mockGuessRepository.Setup(r => r.AddAsync(It.IsAny<Guess>()))
            .Returns(Task.CompletedTask);

        _mockGameRepository.Setup(r => r.UpdateAsync(It.IsAny<Game>()))
            .Returns(Task.CompletedTask);

        var roundAchievement = new UserAchievement
        {
            UserId = game.UserId,
            AchievementId = 1,
            Achievement = new Achievement
            {
                Id = 1,
                Code = "PERFECT_ROUND",
                Name = "Perfect Round",
                Description = "Score 1000 in a round"
            },
            UnlockedAt = DateTime.UtcNow
        };

        var gameAchievement = new UserAchievement
        {
            UserId = game.UserId,
            AchievementId = 2,
            Achievement = new Achievement
            {
                Id = 2,
                Code = "GAME_MASTER",
                Name = "Game Master",
                Description = "Complete a game with high score"
            },
            UnlockedAt = DateTime.UtcNow
        };

        _achievementService
            .Setup(s => s.OnRoundSubmittedAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<double>(),
                It.IsAny<int>()))
            .ReturnsAsync(new List<UserAchievement> { roundAchievement });

        _achievementService
            .Setup(s => s.OnGameFinishedAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync(new List<UserAchievement> { gameAchievement });

        // Act
        var (guessDto, finished, currentRound, totalScore, unlockedAchievements) = await _service.CreateGuessAsync(dto);

        // Assert
        Assert.NotNull(game.FinishedAt);
        Assert.True(game.FinishedAt <= DateTime.UtcNow);
        Assert.True(game.FinishedAt >= DateTime.UtcNow.AddSeconds(-5));
        Assert.Equal(300, game.TotalScore);  
        Assert.Equal(3, game.CurrentRound); 

        // Assert
        Assert.True(finished);
        Assert.Equal(game.CurrentRound, currentRound);
        Assert.Equal(game.TotalScore, totalScore);

        // Assert 
        Assert.NotNull(unlockedAchievements);
        Assert.Equal(2, unlockedAchievements.Count);
        Assert.Contains(unlockedAchievements, a => a.Code == "PERFECT_ROUND");
        Assert.Contains(unlockedAchievements, a => a.Code == "GAME_MASTER");

        _achievementService.Verify(
            s => s.OnGameFinishedAsync(
                game.UserId,
                game.Id,
                300, 
                3),
            Times.Once);
    }

    [Fact]
    public async Task CreateGuessAsync_WithAchievements_MapsThemCorrectly()
    {
        // Arrange
        var dto = new CreateGuessDto
        {
            GameId = Guid.NewGuid(),
            LocationId = 1,
            GuessedLatitude = 10,
            GuessedLongitude = 20,
            DistanceKm = 5,
            Score = 100
        };

        var game = new Game
        {
            Id = dto.GameId,
            UserId = Guid.NewGuid(),
            User = null!,
            FinishedAt = null,
            CurrentRound = 1,
            TotalRounds = 3,
            TotalScore = 0
        };

        var location = new Location
        {
            Id = dto.LocationId,
            Latitude = 50,
            Longitude = 60
        };

        _mockGameRepository.Setup(r => r.GetByIdAsync(dto.GameId))
            .ReturnsAsync(game);

        _mockLocationRepository.Setup(r => r.GetByIdAsync(dto.LocationId))
            .ReturnsAsync(location);

        _mockGuessRepository.Setup(r => r.AddAsync(It.IsAny<Guess>()))
            .Returns(Task.CompletedTask);

        _mockGameRepository.Setup(r => r.UpdateAsync(It.IsAny<Game>()))
            .Returns(Task.CompletedTask);

        var achievements = new List<UserAchievement>
        {
            new UserAchievement
            {
                UserId = game.UserId,
                AchievementId = 1,
                Achievement = new Achievement
                {
                    Id = 1,
                    Code = "FIRST_GUESS",
                    Name = "First Guess",
                    Description = "Make your first guess"
                },
                UnlockedAt = DateTime.UtcNow
            },
            new UserAchievement
            {
                UserId = game.UserId,
                AchievementId = 2,
                Achievement = new Achievement
                {
                    Id = 2,
                    Code = "CLOSE_CALL",
                    Name = "Close Call",
                    Description = "Guess within 10km"
                },
                UnlockedAt = DateTime.UtcNow
            }
        };

        _achievementService
            .Setup(s => s.OnRoundSubmittedAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<double>(),
                It.IsAny<int>()))
            .ReturnsAsync(achievements);

        // Act
        var (guessDto, finished, currentRound, totalScore, unlockedAchievements) = await _service.CreateGuessAsync(dto);

        // Assert
        Assert.NotNull(unlockedAchievements);
        Assert.Equal(2, unlockedAchievements.Count);

        var firstAchievement = unlockedAchievements.First(a => a.Code == "FIRST_GUESS");
        Assert.Equal("First Guess", firstAchievement.Name);
        Assert.Equal("Make your first guess", firstAchievement.Description);

        var secondAchievement = unlockedAchievements.First(a => a.Code == "CLOSE_CALL");
        Assert.Equal("Close Call", secondAchievement.Name);
        Assert.Equal("Guess within 10km", secondAchievement.Description);
    }

    [Fact]
    public async Task CreateGuessAsync_WithNullAchievementInList_SkipsNullEntry()
    {
        // Arrange
        var dto = new CreateGuessDto
        {
            GameId = Guid.NewGuid(),
            LocationId = 1,
            GuessedLatitude = 10,
            GuessedLongitude = 20,
            DistanceKm = 5,
            Score = 100
        };

        var game = new Game
        {
            Id = dto.GameId,
            UserId = Guid.NewGuid(),
            User = null!,
            FinishedAt = null,
            CurrentRound = 1,
            TotalRounds = 3,
            TotalScore = 0
        };

        var location = new Location
        {
            Id = dto.LocationId,
            Latitude = 50,
            Longitude = 60
        };

        _mockGameRepository.Setup(r => r.GetByIdAsync(dto.GameId))
            .ReturnsAsync(game);

        _mockLocationRepository.Setup(r => r.GetByIdAsync(dto.LocationId))
            .ReturnsAsync(location);

        _mockGuessRepository.Setup(r => r.AddAsync(It.IsAny<Guess>()))
            .Returns(Task.CompletedTask);

        _mockGameRepository.Setup(r => r.UpdateAsync(It.IsAny<Game>()))
            .Returns(Task.CompletedTask);

        var achievements = new List<UserAchievement>
        {
            new UserAchievement
            {
                UserId = game.UserId,
                AchievementId = 1,
                Achievement = new Achievement
                {
                    Id = 1,
                    Code = "VALID_ACH",
                    Name = "Valid Achievement",
                    Description = "This is valid"
                },
                UnlockedAt = DateTime.UtcNow
            },
            new UserAchievement
            {
                UserId = game.UserId,
                AchievementId = 2,
                Achievement = null!  
            },
            null!
        };

        _achievementService
            .Setup(s => s.OnRoundSubmittedAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<double>(),
                It.IsAny<int>()))
            .ReturnsAsync(achievements);

        // Act
        var (guessDto, finished, currentRound, totalScore, unlockedAchievements) = await _service.CreateGuessAsync(dto);

        // Assert 
        Assert.NotNull(unlockedAchievements);
        Assert.Single(unlockedAchievements);
        Assert.Equal("VALID_ACH", unlockedAchievements[0].Code);
    }
}
