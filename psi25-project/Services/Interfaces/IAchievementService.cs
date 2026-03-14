using psi25_project.Models;

namespace psi25_project.Services.Interfaces;

public interface IAchievementService
{
    Task<IReadOnlyList<UserAchievement>> OnRoundSubmittedAsync(Guid userId, Guid gameId, int roundNumber, double distanceKm, int score);

    Task<IReadOnlyList<UserAchievement>> OnGameFinishedAsync(Guid userId, Guid gameId, int totalScore, int totalRounds);
    Task<List<AchievementDto>> GetActiveAchievementsAsync();
    Task<List<AchievementDto>> GetAchievementsForUserAsync(Guid userId);
    Task<UserStatsDto> GetUserStatsAsync(Guid userId);
}