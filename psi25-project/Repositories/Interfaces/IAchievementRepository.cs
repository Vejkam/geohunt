using psi25_project.Models;

namespace psi25_project.Repositories.Interfaces;

public interface IAchievementRepository
{
    Task<IReadOnlyList<Achievement>> GetActiveByCodesAsync(IEnumerable<string> codes);
    Task<IReadOnlyList<UserAchievement>> GetUnlockedAsync(Guid userId, IEnumerable<int> achievementIds);
    Task AddNewlyUnlockedAchievementsAsync(IEnumerable<UserAchievement> unlocks, CancellationToken ct = default);
    Task<List<Achievement>> GetActiveAchievementsAsync();
    Task<List<UserAchievement>> GetAchievementsForUserAsync(Guid userId);
}