using psi25_project.Models;
using psi25_project.Services;

namespace Geohunt.Tests.Services;
public class AchievementServiceLogicTests
{
    [Fact]
    public void CalculateStreak_FirstTimePlaying_SetsStreakTo1()
    {
        // Arrange
        DateTime? lastPlayed = null;
        var currentStreak = 0;
        var longestStreak = 0;
        var now = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var (newStreak, isNewLongest) = AchievementService.CalculateStreak(
            lastPlayed,
            currentStreak,
            longestStreak,
            now);

        // Assert
        Assert.Equal(1, newStreak);
        Assert.True(isNewLongest);
    }

    [Fact]
    public void CalculateStreak_PlayedYesterday_IncrementsStreak()
    {
        // Arrange
        var now = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        DateTime? lastPlayed = now.AddDays(-1);
        var currentStreak = 5;
        var longestStreak = 10;

        // Act
        var (newStreak, isNewLongest) = AchievementService.CalculateStreak(
            lastPlayed,
            currentStreak,
            longestStreak,
            now);

        // Assert
        Assert.Equal(6, newStreak);
        Assert.False(isNewLongest);
    }

    [Fact]
    public void CalculateStreak_PlayedToday_KeepsSameStreak()
    {
        // Arrange
        var now = new DateTime(2024, 1, 15, 8, 0, 0, DateTimeKind.Utc);
        DateTime? lastPlayed = now;
        var currentStreak = 5;
        var longestStreak = 10;

        // Act
        var (newStreak, isNewLongest) = AchievementService.CalculateStreak(
            lastPlayed,
            currentStreak,
            longestStreak,
            now);

        // Assert
        Assert.Equal(currentStreak, newStreak);
        Assert.False(isNewLongest);
    }

    [Fact]
    public void CalculateStreak_PlayedLongAgo_ResetsStreakTo1()
    {
        // Arrange
        var now = new DateTime(2024, 1, 15, 18, 0, 0, DateTimeKind.Utc);
        DateTime? lastPlayed = now.AddDays(-5);
        var currentStreak = 5;
        var longestStreak = 10;

        // Act
        var (newStreak, isNewLongest) = AchievementService.CalculateStreak(
            lastPlayed,
            currentStreak,
            longestStreak,
            now);

        // Assert
        Assert.Equal(1, newStreak);
        Assert.False(isNewLongest);
    }

    [Fact]
    public void CalculateStreak_NewStreakGreaterThanLongest_FlagsNewLongest()
    {
        // Arrange
        var now = new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc);
        DateTime? lastPlayed = now.AddDays(-1);
        var currentStreak = 5;
        var longestStreak = 5;

        // Act
        var (newStreak, isNewLongest) = AchievementService.CalculateStreak(
            lastPlayed,
            currentStreak,
            longestStreak,
            now);

        // Assert
        Assert.Equal(6, newStreak);
        Assert.True(isNewLongest);
    }

    [Fact]
    public void CalculateStreak_NewStreakNotGreaterThanLongest_DoesNotFlagNewLongest()
    {
        // Arrange
        var now = new DateTime(2024, 1, 15, 9, 0, 0, DateTimeKind.Utc);
        DateTime? lastPlayed = now.AddDays(-1);
        var currentStreak = 5;
        var longestStreak = 7;

        // Act
        var (newStreak, isNewLongest) = AchievementService.CalculateStreak(
            lastPlayed,
            currentStreak,
            longestStreak,
            now);

        // Assert
        Assert.Equal(6, newStreak);
        Assert.False(isNewLongest);
    }

    [Fact]
    public void EvaluateRoundAchievements_FirstGuess_AddsFirstGuessCode()
    {
        // Arrange
        var isFirstGuess = true;
        var distance = 1000;
        int score = 100;

        // Act
        var result = AchievementService.EvaluateRoundAchievements(isFirstGuess, distance, score);

        // Assert
        Assert.Contains(AchievementCodes.FirstGuess, result);
    }

    [Fact]
    public void EvaluateRoundAchievements_BullseyeDistance_AddsBullseyeCode()
    {
        // Arrange
        var isFirstGuess = false;
        var distance = 0.05;
        var score = 4999;

        // Act
        var result = AchievementService.EvaluateRoundAchievements(isFirstGuess, distance, score);

        // Assert
        Assert.Contains(AchievementCodes.Bullseye100m, result);
    }

    [Fact]
    public void EvaluateRoundAchievements_Within1Km_AddsNear1kmCode()
    {
        // Arrange
        var isFirstGuess = false;
        var distance = 0.5;
        var score = 4900;

        // Act
        var result = AchievementService.EvaluateRoundAchievements(isFirstGuess, distance, score);

        // Assert
        Assert.Contains(AchievementCodes.Near1km, result);
    }

    [Fact]
    public void EvaluateRoundAchievements_FarDistance_AddsNoDistanceAchievements()
    {
        // Arrange
        var isFirstGuess = false;
        var distance = 3456;
        var score = 678;

        // Act
        var result = AchievementService.EvaluateRoundAchievements(isFirstGuess, distance, score);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void EvaluateRoundAchievements_CombinationOfConditions_AddsAllRelevantCodes()
    {
        // Arrange
        var isFirstGuess = true;
        var distance = 0.5;
        var score = 4900;

        // Act
        var result = AchievementService.EvaluateRoundAchievements(isFirstGuess, distance, score);

        // Assert
        Assert.Contains(AchievementCodes.Near1km, result);
        Assert.Contains(AchievementCodes.FirstGuess, result);
    }

    [Fact]
    public void EvaluateGameAchievements_TotalScoreAbove10k_AddsScore10k()
    {
        // Arrange
        var stats = new UserStats();
        var totalScore = 15000;
        var guesses = new List<Guess>();
        var totalRounds = 3;
        DateTime now = new DateTime(2025, 12, 06, 16, 00, 00, DateTimeKind.Utc);        

        // Act
        var result = AchievementService.EvaluateGameAchievements(stats, totalScore, guesses, totalRounds, now);

        // Assert
        Assert.Contains(AchievementCodes.Score10k, result);
    }

    [Fact]
    public void EvaluateGameAchievements_AllRoundsWithin1Km_AddsCleanSweep()
    {
        // Arrange
        var stats = new UserStats();
        var totalScore = 10000;
        var guesses = new List<Guess>
        {
            new Guess { DistanceKm = 0.05, Game = null!, Location = null! },
            new Guess { DistanceKm = 0.5, Game = null!, Location = null! },
            new Guess { DistanceKm = 0.99, Game = null!, Location = null! },
        };
        var totalRounds = 3;
        DateTime now = new DateTime(2025, 12, 06, 16, 00, 00, DateTimeKind.Utc);        

        // Act
        var result = AchievementService.EvaluateGameAchievements(stats, totalScore, guesses, totalRounds, now);

        // Assert
        Assert.Contains(AchievementCodes.CleanSweep, result);
    }

    [Fact]
    public void EvaluateGameAchievements_TotalGamesEquals100_AddsMarathoner()
    {
        // Arrange
        var stats = new UserStats
        {
            TotalGames = 100
        };
        var totalScore = 15000;
        var guesses = new List<Guess>();
        var totalRounds = 3;
        DateTime now = new DateTime(2025, 12, 06, 16, 00, 00, DateTimeKind.Utc);        

        // Act
        var result = AchievementService.EvaluateGameAchievements(stats, totalScore, guesses, totalRounds, now);

        // Assert
        Assert.Contains(AchievementCodes.TheMarathoner, result);
    }

    [Fact]
    public void EvaluateGameAchievements_StreakEquals30_AddsStreakMaster()
    {
        // Arrange
        var stats = new UserStats
        {
            CurrentStreakDays = 30
        };
        var totalScore = 15000;
        var guesses = new List<Guess>();
        var totalRounds = 3;
        DateTime now = new DateTime(2025, 12, 06, 16, 00, 00, DateTimeKind.Utc);        

        // Act
        var result = AchievementService.EvaluateGameAchievements(stats, totalScore, guesses, totalRounds, now);

        // Assert
        Assert.Contains(AchievementCodes.StreakMaster, result);
    }

    [Fact]
    public void EvaluateGameAchievements_LateNightGame_AddsLateNightPlayer()
    {
        // Arrange
        var stats = new UserStats();
        var totalScore = 15000;
        var guesses = new List<Guess>();
        var totalRounds = 3;
        DateTime now = new DateTime(2025, 12, 06, 3, 00, 00, DateTimeKind.Utc);        

        // Act
        var result = AchievementService.EvaluateGameAchievements(stats, totalScore, guesses, totalRounds, now);

        // Assert
        Assert.Contains(AchievementCodes.LateNightPlayer, result);
    }

    [Fact]
    public void EvaluateGameAchievements_NoConditionsMet_ReturnsEmptyList()
    {
        // Arrange
        var stats = new UserStats();
        var totalScore = 3045;
        var guesses = new List<Guess>();
        var totalRounds = 3;
        DateTime now = new DateTime(2025, 12, 06, 16, 00, 00, DateTimeKind.Utc);        

        // Act
        var result = AchievementService.EvaluateGameAchievements(stats, totalScore, guesses, totalRounds, now);

        // Assert
        Assert.Empty(result);
    }
}
