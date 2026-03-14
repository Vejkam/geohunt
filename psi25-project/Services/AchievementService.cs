using psi25_project.Models;
using psi25_project.Repositories.Interfaces;
using psi25_project.Services.Interfaces;

namespace psi25_project.Services;

public class AchievementService : IAchievementService
{
    private readonly IAchievementRepository _achievementRepository;
    private readonly IUserStatsRepository _userStatsRepository;
    private readonly IGuessRepository _guessRepository;

    private static readonly string[] RoundSubmittedCodes = new[] 
    {
        AchievementCodes.FirstGuess,
        AchievementCodes.Bullseye100m,
        AchievementCodes.Near1km
    };
    private static readonly string[] GameFinishedCodes = new[]
    {
        AchievementCodes.Score10k,
        AchievementCodes.CleanSweep,
        AchievementCodes.TheMarathoner,
        AchievementCodes.StreakMaster,
        AchievementCodes.LateNightPlayer
    };

    public AchievementService(
        IAchievementRepository achievementRepository,
        IUserStatsRepository userStatsRepository,
        IGuessRepository guessRepository)
    {
        _achievementRepository = achievementRepository;
        _userStatsRepository = userStatsRepository;
        _guessRepository = guessRepository;
    }

    public async Task<IReadOnlyList<UserAchievement>> OnRoundSubmittedAsync(Guid userId, Guid gameId, int roundNumber, double distanceKm, int score)
    {
        var catalog = await _achievementRepository.GetActiveByCodesAsync(RoundSubmittedCodes);
        
        var stats = await _userStatsRepository.GetOrCreateAsync(userId);
        var isFirstGuess = stats.TotalGuesses == 0;

        stats.TotalGuesses += 1;
        await _userStatsRepository.UpdateAsync(stats);

        var toUnlockCodes = EvaluateRoundAchievements(isFirstGuess, distanceKm, score);

        return await UnlockAchievementsAsync(userId, toUnlockCodes, catalog);
    }

    public async Task<IReadOnlyList<UserAchievement>> OnGameFinishedAsync(Guid userId, Guid gameId, int totalScore, int totalRounds)
    {
        var catalog = await _achievementRepository.GetActiveByCodesAsync(GameFinishedCodes);
        var guesses = await _guessRepository.GetGuessesByGameAsync(gameId);
        var stats = await UpdateStatsForGameAsync(userId, totalScore);
    
        var toUnlockCodes = EvaluateGameAchievements(stats, totalScore, guesses, totalRounds, DateTime.UtcNow);

        return await UnlockAchievementsAsync(userId, toUnlockCodes, catalog);
    }

    public async Task<List<AchievementDto>> GetActiveAchievementsAsync()
    {
        var activeAchievements = await _achievementRepository.GetActiveAchievementsAsync();

        if (activeAchievements == null || activeAchievements.Count == 0)
            return new List<AchievementDto>();

        return MapToDto(activeAchievements);
    }

    public async Task<List<AchievementDto>> GetAchievementsForUserAsync(Guid userId)
    {
        var unlockedAchievements = await _achievementRepository.GetAchievementsForUserAsync(userId);

        if (unlockedAchievements == null || unlockedAchievements.Count == 0)
            return new List<AchievementDto>();

        var dtos = unlockedAchievements.Select(ua => new AchievementDto
        {
            Code = ua.Achievement?.Code ?? string.Empty,
            Name = ua.Achievement?.Name ?? string.Empty,
            Description = ua.Achievement?.Description ?? string.Empty,
            UnlockedAt = ua.UnlockedAt
        }).ToList();

        return dtos;
    }

    private static List<AchievementDto> MapToDto(List<Achievement> achievements)
    {
        var dtos = new List<AchievementDto>();
        foreach (var a in achievements)
        {
            dtos.Add(new AchievementDto
            {
                Code = a.Code,
                Name = a.Name,
                Description = a.Description
            });
        }
        return dtos;
    }

    public async Task<UserStatsDto> GetUserStatsAsync(Guid userId)
    {
        var stats = await _userStatsRepository.GetOrCreateAsync(userId);

        var dto = new UserStatsDto 
        {
            TotalGames = stats.TotalGames,
            CurrentStreakDays = stats.CurrentStreakDays,
            LongestStreakDays = stats.LongestStreakDays
        };

        return dto;
    }

    internal static List<string> EvaluateRoundAchievements(bool isFirstGuess, double distanceKm, int score)
    {
        var toUnlockCodes = new List<string>();

        if (isFirstGuess)
            toUnlockCodes.Add(AchievementCodes.FirstGuess);
        if (distanceKm <= 0.1)
            toUnlockCodes.Add(AchievementCodes.Bullseye100m);
        if (distanceKm <= 1.0)
            toUnlockCodes.Add(AchievementCodes.Near1km);

        return toUnlockCodes;
    }

    internal static List<string> EvaluateGameAchievements(
        UserStats stats,
        int totalScore,
        IEnumerable<Guess> guesses,
        int totalRounds,
        DateTime currentTime)
    {
        var toUnlockCodes = new List<string>();

        if (totalScore >= 10_000)
            toUnlockCodes.Add(AchievementCodes.Score10k);

        if (guesses.Count() == totalRounds && guesses.All(g => g.DistanceKm <= 1.0))
            toUnlockCodes.Add(AchievementCodes.CleanSweep);

        if (stats.TotalGames == 100)
            toUnlockCodes.Add(AchievementCodes.TheMarathoner);

        if (stats.CurrentStreakDays == 30)
            toUnlockCodes.Add(AchievementCodes.StreakMaster);

        if (currentTime.Hour >= 0 && currentTime.Hour <= 6)
            toUnlockCodes.Add(AchievementCodes.LateNightPlayer);
        
        return toUnlockCodes;
    }

    private async Task<UserStats> UpdateStatsForGameAsync(Guid userId, int totalScore)
    {
        // Get / create user stats
        var stats = await _userStatsRepository.GetOrCreateAsync(userId);
        var now = DateTime.UtcNow;

        stats.TotalGames += 1;

        if (totalScore > stats.BestGameScore)
            stats.BestGameScore = totalScore;

        var (newStreak, isNewLongest) = CalculateStreak(stats.LastPlayedDateUtc, stats.CurrentStreakDays, stats.LongestStreakDays, now);
        
        stats.CurrentStreakDays = newStreak;
        stats.LastPlayedDateUtc = now;

        if (isNewLongest)
            stats.LongestStreakDays = newStreak;

        await _userStatsRepository.UpdateAsync(stats);

        return stats;
    }

    internal static (int newStreak, bool isNewLongest) CalculateStreak(
        DateTime? lastPlayedDate,
        int currentStreak,
        int longestStreak,
        DateTime now)
    {
        var today = now.Date;
        var lastPlayed = lastPlayedDate?.Date;

        int newStreak;

        if (lastPlayed == null)
        {
            newStreak = 1;
        } else if (lastPlayed == today)
        {
            newStreak = currentStreak;
        } else if (lastPlayed == today.AddDays(-1))
        {
            newStreak = currentStreak + 1;
        } else
        {
            newStreak = 1;
        }

        bool isNewLongest = newStreak > longestStreak;

        return (newStreak, isNewLongest);
    }

    // Common flow: unlock achievements and return results
    private async Task<IReadOnlyList<UserAchievement>> UnlockAchievementsAsync(
        Guid userId, 
        List<string> achievementCodes, 
        IReadOnlyList<Achievement> catalog)
    {
        if (achievementCodes.Count == 0) 
            return Array.Empty<UserAchievement>();

        var targetAchievements = MapCodesToAchievements(achievementCodes, catalog);
        if (targetAchievements.Count == 0)
            return Array.Empty<UserAchievement>();

        var newUnlocks = await FilterAlreadyUnlockedAsync(userId, targetAchievements);
        if (newUnlocks.Count == 0)
            return Array.Empty<UserAchievement>();

        return await SaveAndReloadUnlockedAchievementsAsync(userId, newUnlocks);
    }

    private static List<Achievement> MapCodesToAchievements(
        List<string> achievementCodes, 
        IReadOnlyList<Achievement> catalog)
    {
        return catalog
            .Where(a => achievementCodes.Contains(a.Code))
            .ToList();
    }

    private async Task<List<UserAchievement>> FilterAlreadyUnlockedAsync(
        Guid userId, 
        List<Achievement> targetAchievements)
    {
        var alreadyUnlocked = await _achievementRepository.GetUnlockedAsync(
            userId,
            targetAchievements.Select(a => a.Id)
        );

        var alreadyUnlockedIds = alreadyUnlocked
            .Select(ua => ua.AchievementId)
            .ToHashSet();

        var newUnlocks = targetAchievements
            .Where(a => !alreadyUnlockedIds.Contains(a.Id))
            .Select(a => new UserAchievement
            {
                UserId = userId,
                AchievementId = a.Id,
                UnlockedAt = DateTime.UtcNow
            })
            .ToList();

        return newUnlocks;
    }

    private async Task<IReadOnlyList<UserAchievement>> SaveAndReloadUnlockedAchievementsAsync(
        Guid userId, 
        List<UserAchievement> newUnlocks)
    {
        await _achievementRepository.AddNewlyUnlockedAchievementsAsync(newUnlocks);

        var unlockedWithAchievements = await _achievementRepository.GetUnlockedAsync(
            userId,
            newUnlocks.Select(u => u.AchievementId)
        );

        return unlockedWithAchievements;
    }
}