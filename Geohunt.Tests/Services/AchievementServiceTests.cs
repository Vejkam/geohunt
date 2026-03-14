using Moq;
using psi25_project.Models;
using psi25_project.Repositories.Interfaces;
using psi25_project.Services;

namespace Geohunt.Tests.Services;

public class AchievementServiceTests
{
    private readonly Mock<IAchievementRepository> _achievementRepo = new();
    private readonly Mock<IUserStatsRepository> _userStatsRepo = new();
    private readonly Mock<IGuessRepository> _guessRepo = new();
    private readonly AchievementService _service;

    public AchievementServiceTests()
    {
        _service = new AchievementService(
            _achievementRepo.Object,
            _userStatsRepo.Object,
            _guessRepo.Object);
    }

    [Fact]
    public async Task OnRoundSubmittedAsync_FirstGuess_UnlocksFirstGuessAchievement()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        
        var firstGuessAchievement = new Achievement 
        { 
            Id = 1, 
            Code = AchievementCodes.FirstGuess, 
            Name = "First guess", 
            Description = "Made your first guess" 
        };

        _achievementRepo
            .Setup(r => r.GetActiveByCodesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<Achievement> { firstGuessAchievement });

        _userStatsRepo
            .Setup(r => r.GetOrCreateAsync(userId))
            .ReturnsAsync(new UserStats
            {
                UserId = userId,
                TotalGuesses = 0
            });

        _achievementRepo
            .Setup(r => r.GetUnlockedAsync(userId, It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<UserAchievement>());

        _achievementRepo
            .Setup(r => r.AddNewlyUnlockedAchievementsAsync(It.IsAny<List<UserAchievement>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _achievementRepo
            .SetupSequence(r => r.GetUnlockedAsync(userId, It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<UserAchievement>()) 
            .ReturnsAsync(new List<UserAchievement>
            {
                new UserAchievement
                {
                    UserId = userId,
                    AchievementId = 1,
                    Achievement = firstGuessAchievement,
                    UnlockedAt = DateTime.UtcNow
                }
            });

        // Act
        var result = await _service.OnRoundSubmittedAsync(
            userId, gameId,
            roundNumber: 1,
            distanceKm: 10,
            score: 1000);

        // Assert
        Assert.Single(result);
        Assert.Equal(AchievementCodes.FirstGuess, result[0].Achievement!.Code);
        _userStatsRepo.Verify(r => r.UpdateAsync(It.IsAny<UserStats>()), Times.Once);
    }

    [Fact]
    public async Task OnRoundSubmittedAsync_CloseDistance_UnlockesBullseyeAndNear1km()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        
        var bullseyeAchievement = new Achievement 
        { 
            Id = 1, 
            Code = AchievementCodes.Bullseye100m, 
            Name = "Bullseye", 
            Description = "Guess within 100m" 
        };

        var near1km = new Achievement 
        { 
            Id = 2, 
            Code = AchievementCodes.Near1km, 
            Name = "Near1km", 
            Description = "Guess within 1km" 
        };

        _achievementRepo
            .Setup(r => r.GetActiveByCodesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<Achievement> { bullseyeAchievement, near1km });

        _userStatsRepo
            .Setup(r => r.GetOrCreateAsync(userId))
            .ReturnsAsync(new UserStats
            {
                UserId = userId,
                TotalGuesses = 0
            });

        _achievementRepo
            .Setup(r => r.AddNewlyUnlockedAchievementsAsync(It.IsAny<List<UserAchievement>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _achievementRepo
            .SetupSequence(r => r.GetUnlockedAsync(userId, It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<UserAchievement>()) 
            .ReturnsAsync(new List<UserAchievement>
            {
                new UserAchievement
                {
                    UserId = userId,
                    AchievementId = 1,
                    Achievement = bullseyeAchievement,
                    UnlockedAt = DateTime.UtcNow
                },
                new UserAchievement
                {
                    UserId = userId,
                    AchievementId = 2,
                    Achievement = near1km,
                    UnlockedAt = DateTime.UtcNow
                }
            });

        // Act
        var result = await _service.OnRoundSubmittedAsync(
            userId, gameId,
            roundNumber: 1,
            distanceKm: 0.05,
            score: 4900);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Collection(result,
            item1 => Assert.Equal(AchievementCodes.Bullseye100m, item1.Achievement!.Code),
            item2 => Assert.Equal(AchievementCodes.Near1km, item2.Achievement!.Code)
        );
        _userStatsRepo.Verify(r => r.UpdateAsync(It.IsAny<UserStats>()), Times.Once);
    }

    [Fact]
    public async Task OnRoundSubmittedAsync_NoConditionsMet_DoesNotUnlockAnything()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();

        _achievementRepo
            .Setup(r => r.GetActiveByCodesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<Achievement>
            {
                new Achievement { Id = 1, Code = AchievementCodes.FirstGuess, Name = "First Guess", Description = ""},
                new Achievement { Id = 2, Code = AchievementCodes.Bullseye100m, Name = "Bullseye", Description = ""},
                new Achievement { Id = 3, Code = AchievementCodes.Near1km, Name = "Near 1km", Description = ""},
            });

        _userStatsRepo
            .Setup(r => r.GetOrCreateAsync(userId))
            .ReturnsAsync(new UserStats
            {
                UserId = userId,
                TotalGuesses = 10 // not first guess
            });

        // Act
        var result = await _service.OnRoundSubmittedAsync(
            userId, gameId,
            roundNumber: 1,
            distanceKm: 5.0,
            score: 0);

        // Assert
        Assert.Empty(result);

        _achievementRepo.Verify(
            r => r.AddNewlyUnlockedAchievementsAsync(
                It.IsAny<List<UserAchievement>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _achievementRepo.Verify(
            r => r.GetUnlockedAsync(
                It.IsAny<Guid>(),
                It.IsAny<IEnumerable<int>>()),
            Times.Never);

        _userStatsRepo.Verify(
            r => r.UpdateAsync(It.IsAny<UserStats>()),
            Times.Once);
    }

    [Fact]
    public async Task OnRoundSubmittedAsync_UpdatesUserStatsTotalGuesses()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();

        _achievementRepo
            .Setup(r => r.GetActiveByCodesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<Achievement>());
        
        _userStatsRepo
            .Setup(r => r.GetOrCreateAsync(userId))
            .ReturnsAsync(new UserStats
            {
                UserId = userId,
                TotalGuesses = 10
            });

        // Act
        var result = await _service.OnRoundSubmittedAsync(
            userId, gameId,
            roundNumber: 1,
            distanceKm: 5.0,
            score: 0);

        // Assert
        _userStatsRepo.Verify(
            r => r.UpdateAsync(It.Is<UserStats>(stats => stats.TotalGuesses == 11)),
            Times.Once);    
    }

    [Fact]
    public async Task OnRoundSubmittedAsync_DoesNotDuplicateAlreadyUnlockedAchievements()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        
        var firstGuessAchievement = new Achievement 
        { 
            Id = 1, 
            Code = AchievementCodes.FirstGuess, 
            Name = "First guess", 
            Description = "Made your first guess" 
        };

        _achievementRepo
            .Setup(r => r.GetActiveByCodesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<Achievement> { firstGuessAchievement });

        _userStatsRepo
            .Setup(r => r.GetOrCreateAsync(userId))
            .ReturnsAsync(new UserStats
            {
                UserId = userId,
                TotalGuesses = 0
            });
        
        _achievementRepo
            .Setup(r => r.GetUnlockedAsync(userId, It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<UserAchievement>()
            {
                new UserAchievement
                {
                    UserId = userId,
                    AchievementId = 1,
                    Achievement = firstGuessAchievement,
                    UnlockedAt = DateTime.UtcNow
                }
            });

        // Act
        var result = await _service.OnRoundSubmittedAsync(
            userId, gameId,
            roundNumber: 1,
            distanceKm: 1234,
            score: 1456);

        // Assert
        Assert.Empty(result);

        _achievementRepo.Verify(
            r => r.AddNewlyUnlockedAchievementsAsync(
                It.IsAny<List<UserAchievement>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _achievementRepo.Verify(
            r => r.GetUnlockedAsync(
                It.IsAny<Guid>(),
                It.IsAny<IEnumerable<int>>()),
            Times.Once);
    }

    [Fact]
    public async Task OnGameFinishedAsync_HighScore_UnlocksScore10k()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        
        var scoreAchievement = new Achievement 
        { 
            Id = 1, 
            Code = AchievementCodes.Score10k, 
            Name = "Score 10k points", 
            Description = "Game's score >= 10k" 
        };

        _achievementRepo
            .Setup(r => r.GetActiveByCodesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<Achievement> { scoreAchievement });

        _guessRepo
            .Setup(r => r.GetGuessesByGameAsync(gameId))
            .ReturnsAsync(new List<Guess>());

        _userStatsRepo
            .Setup(r => r.GetOrCreateAsync(userId))
            .ReturnsAsync(new UserStats
            {
                UserId = userId,
                TotalGuesses = 0,
                BestGameScore = 1000
            });

        _achievementRepo
            .Setup(r => r.AddNewlyUnlockedAchievementsAsync(It.IsAny<List<UserAchievement>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _achievementRepo
            .SetupSequence(r => r.GetUnlockedAsync(userId, It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<UserAchievement>()) 
            .ReturnsAsync(new List<UserAchievement>
            {
                new UserAchievement
                {
                    UserId = userId,
                    AchievementId = 1,
                    Achievement = scoreAchievement,
                    UnlockedAt = DateTime.UtcNow
                }
            });

        // Act
        var result = await _service.OnGameFinishedAsync(
            userId, gameId,
            totalScore: 11000,
            totalRounds: 3);

        // Assert
        Assert.Single(result);
        Assert.Equal(AchievementCodes.Score10k, result[0].Achievement!.Code);
        _userStatsRepo.Verify(r => r.UpdateAsync(It.IsAny<UserStats>()), Times.Once);
    }

    [Fact]
    public async Task OnGameFinishedAsync_StatsTriggerStreakMasterOrMarathoner_UnlocksRelevantAchievements()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();
        
        var marathonerAchievement = new Achievement 
        { 
            Id = 1, 
            Code = AchievementCodes.TheMarathoner, 
            Name = "The Marahoner", 
            Description = "Total games == 100" 
        };

        var streakAchievement = new Achievement
        {
            Id = 2,
            Code = AchievementCodes.StreakMaster,
            Name = "Streak Master",
            Description = "Streak == 30 days"
        };

        _achievementRepo
            .Setup(r => r.GetActiveByCodesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<Achievement> { marathonerAchievement, streakAchievement });

        _guessRepo
            .Setup(r => r.GetGuessesByGameAsync(gameId))
            .ReturnsAsync(new List<Guess>());

        _userStatsRepo
            .Setup(r => r.GetOrCreateAsync(userId))
            .ReturnsAsync(new UserStats
            {
                TotalGames = 99,
                CurrentStreakDays = 29,
                LongestStreakDays = 30,
                LastPlayedDateUtc = DateTime.UtcNow.AddDays(-1)
            });

        _achievementRepo
            .SetupSequence(r => r.GetUnlockedAsync(userId, It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(new List<UserAchievement>())
            .ReturnsAsync(new List<UserAchievement> 
            {
                new UserAchievement
                {
                    UserId = userId,
                    AchievementId = 1,
                    Achievement = marathonerAchievement,
                    UnlockedAt = DateTime.UtcNow
                },
                new UserAchievement
                {
                    UserId = userId,
                    AchievementId = 2,
                    Achievement = streakAchievement,
                    UnlockedAt = DateTime.UtcNow
                }
            });

        _achievementRepo
            .Setup(r => r.AddNewlyUnlockedAchievementsAsync(It.IsAny<List<UserAchievement>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);


        // Act
        var result = await _service.OnGameFinishedAsync(
            userId, gameId,
            totalScore: 3456,
            totalRounds: 2
        );

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Collection(result,
            item1 => Assert.Equal(AchievementCodes.TheMarathoner, item1.Achievement!.Code),
            item2 => Assert.Equal(AchievementCodes.StreakMaster, item2.Achievement!.Code)
        );
        _userStatsRepo.Verify(r => r.UpdateAsync(It.IsAny<UserStats>()), Times.Once);
    }

    [Fact]
    public async Task OnGameFinishedAsync_NoNewAchievements_DoesNotSaveUnlocks()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();

        var gameAchievements = new List<Achievement>
        {
            new Achievement
            {
                Code = AchievementCodes.Score10k,
                Name = "Score 10k",
                Description = ""
            },
            new Achievement
            {
                Code = AchievementCodes.CleanSweep,
                Name = "Clean Sweep",
                Description = ""
            },
            new Achievement
            {
                Code = AchievementCodes.TheMarathoner,
                Name = "The Marahoner",
                Description = ""
            },
            new Achievement
            {
                Code = AchievementCodes.StreakMaster,
                Name = "Streak Master",
                Description = ""
            },
            new Achievement
            {
                Code = AchievementCodes.LateNightPlayer,
                Name = "Late Night Player",
                Description = ""
            }
        };

        _achievementRepo
            .Setup(r => r.GetActiveByCodesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(gameAchievements);

        _guessRepo
            .Setup(r => r.GetGuessesByGameAsync(gameId))
            .ReturnsAsync(new List<Guess>
            {
                new() { DistanceKm = 5.0, Game = null!, Location = null! },
                new() { DistanceKm = 2.0, Game = null!, Location = null! }
            });

        _userStatsRepo
            .Setup(r => r.GetOrCreateAsync(userId))
            .ReturnsAsync(new UserStats
            {
                UserId = userId,
                TotalGames = 10,
                CurrentStreakDays = 5,
                LongestStreakDays = 5,
                LastPlayedDateUtc = DateTime.UtcNow.AddDays(-10),
                BestGameScore = 1000
            });
        
        var result = await _service.OnGameFinishedAsync(
            userId,
            gameId,
            totalScore: 5000,   
            totalRounds: 5);  

        // Assert
        Assert.Empty(result);

        _achievementRepo.Verify(
            r => r.AddNewlyUnlockedAchievementsAsync(
                It.IsAny<List<UserAchievement>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _achievementRepo.Verify(
            r => r.GetUnlockedAsync(
                It.IsAny<Guid>(),
                It.IsAny<IEnumerable<int>>()),
            Times.Never);
    }

    [Fact]
    public async Task OnGameFinishedAsync_CallsUpdateStatsForGameAndPersistsChanges()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var gameId = Guid.NewGuid();

        _achievementRepo
            .Setup(r => r.GetActiveByCodesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<Achievement>());

        _guessRepo
            .Setup(r => r.GetGuessesByGameAsync(gameId))
            .ReturnsAsync(new List<Guess>());

        var initialStats = new UserStats
        {
            UserId = userId,
            TotalGames = 4,
            CurrentStreakDays = 0,
            LongestStreakDays = 0,
            LastPlayedDateUtc = null,
            BestGameScore = 500
        };

        var originalTotalGames = initialStats.TotalGames;
        var originalBestGameScore = initialStats.BestGameScore;

        _userStatsRepo
            .Setup(r => r.GetOrCreateAsync(userId))
            .ReturnsAsync(initialStats);

        UserStats? updatedStats = null;

        _userStatsRepo
            .Setup(r => r.UpdateAsync(It.IsAny<UserStats>()))
            .Callback<UserStats>(s => updatedStats = s)
            .Returns(Task.CompletedTask);

        var newScore = 900;

        // Act
        await _service.OnGameFinishedAsync(
            userId,
            gameId,
            totalScore: newScore,
            totalRounds: 3);

        // Assert
        _userStatsRepo.Verify(r => r.UpdateAsync(It.IsAny<UserStats>()), Times.Once);
        Assert.NotNull(updatedStats);

        Assert.Equal(originalTotalGames + 1, updatedStats!.TotalGames);
        Assert.Equal(newScore, updatedStats.BestGameScore);

        Assert.NotNull(updatedStats.LastPlayedDateUtc);
        Assert.True(updatedStats.CurrentStreakDays >= 1);

        _achievementRepo.Verify(
            r => r.AddNewlyUnlockedAchievementsAsync(
                It.IsAny<List<UserAchievement>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetActiveAchievementsAsync_NoActiveAchievements_ReturnsEmptyList()
    {
        // Arrange
        _achievementRepo
            .Setup(r => r.GetActiveAchievementsAsync())
            .ReturnsAsync(new List<Achievement>());

        // Act
        var result = await _service.GetActiveAchievementsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetActiveAchievementsAsync_ReturnsMappedAchievementDtos()
    {
        // Arrange
        var achievements = new List<Achievement>
        {
            new Achievement
            {
                Id = 1,
                Code = AchievementCodes.FirstGuess,
                Name = "First Guess",
                Description = "Made your first guess",
                IsActive = true
            },
            new Achievement
            {
                Id = 2,
                Code = AchievementCodes.Bullseye100m,
                Name = "Bullseye",
                Description = "Guessed within 100 meters",
                IsActive = true
            },
            new Achievement
            {
                Id = 3,
                Code = AchievementCodes.Score10k,
                Name = "High Scorer",
                Description = "Scored 10,000 points",
                IsActive = true
            }
        };

        _achievementRepo
            .Setup(r => r.GetActiveAchievementsAsync())
            .ReturnsAsync(achievements);

        // Act
        var result = await _service.GetActiveAchievementsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(achievements.Count, result.Count);

        for (int i = 0; i < achievements.Count; i++)
        {
            Assert.Equal(achievements[i].Code, result[i].Code);
            Assert.Equal(achievements[i].Name, result[i].Name);
            Assert.Equal(achievements[i].Description, result[i].Description);
        }

        Assert.Null(result[0].UnlockedAt);
    }

    [Fact]
    public async Task GetAchievementsForUserAsync_NoUnlockedAchievements_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _achievementRepo
            .SetupSequence(r => r.GetAchievementsForUserAsync(userId))
            .ReturnsAsync(new List<UserAchievement>())
            .ReturnsAsync(new List<UserAchievement>());

        // Act
        var resultWhenNull = await _service.GetAchievementsForUserAsync(userId);
        var resultWhenEmpty = await _service.GetAchievementsForUserAsync(userId);

        // Assert
        Assert.NotNull(resultWhenNull);
        Assert.Empty(resultWhenNull);

        Assert.NotNull(resultWhenEmpty);
        Assert.Empty(resultWhenEmpty);

        _achievementRepo.Verify(r => r.GetAchievementsForUserAsync(userId), Times.Exactly(2));
    }

    [Fact]
    public async Task GetAchievementsForUserAsync_MapsUserAchievementsToDtos()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var unlockedAt1 = new DateTime(2024, 1, 10, 12, 0, 0, DateTimeKind.Utc);
        var unlockedAt2 = new DateTime(2024, 2, 5, 18, 30, 0, DateTimeKind.Utc);

        var uaList = new List<UserAchievement>
        {
            new()
            {
                UserId = userId,
                AchievementId = 1,
                Achievement = new Achievement
                {
                    Id = 1,
                    Code = "ACH_CODE_1",
                    Name = "First Achievement",
                    Description = "First achievement description"
                },
                UnlockedAt = unlockedAt1
            },
            new()
            {
                UserId = userId,
                AchievementId = 2,
                Achievement = new Achievement
                {
                    Id = 2,
                    Code = "ACH_CODE_2",
                    Name = "Second Achievement",
                    Description = "Second achievement description"
                },
                UnlockedAt = unlockedAt2
            }
        };

        _achievementRepo
            .Setup(r => r.GetAchievementsForUserAsync(userId))
            .ReturnsAsync(uaList);

        // Act
        var result = await _service.GetAchievementsForUserAsync(userId);

        // Assert
        Assert.Equal(uaList.Count, result.Count);

        Assert.Collection(result,
            dto1 =>
            {
                Assert.Equal(uaList[0].Achievement!.Code, dto1.Code);
                Assert.Equal(uaList[0].Achievement!.Name, dto1.Name);
                Assert.Equal(uaList[0].Achievement!.Description, dto1.Description);
                Assert.Equal(uaList[0].UnlockedAt, dto1.UnlockedAt);
            },
            dto2 =>
            {
                Assert.Equal(uaList[1].Achievement!.Code, dto2.Code);
                Assert.Equal(uaList[1].Achievement!.Name, dto2.Name);
                Assert.Equal(uaList[1].Achievement!.Description, dto2.Description);
                Assert.Equal(uaList[1].UnlockedAt, dto2.UnlockedAt);
            });

        _achievementRepo.Verify(r => r.GetAchievementsForUserAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetUserStatsAsync_ReturnsStatsDtoFromRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var stats = new UserStats
        {
            UserId = userId,
            TotalGames = 12,
            CurrentStreakDays = 5,
            LongestStreakDays = 8
        };

        _userStatsRepo
            .Setup(r => r.GetOrCreateAsync(userId))
            .ReturnsAsync(stats);

        // Act
        var dto = await _service.GetUserStatsAsync(userId);

        // Assert
        Assert.Equal(stats.TotalGames, dto.TotalGames);
        Assert.Equal(stats.CurrentStreakDays, dto.CurrentStreakDays);
        Assert.Equal(stats.LongestStreakDays, dto.LongestStreakDays);

        _userStatsRepo.Verify(r => r.GetOrCreateAsync(userId), Times.Once);
        _userStatsRepo.VerifyNoOtherCalls();
    }
}
