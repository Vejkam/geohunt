using Microsoft.EntityFrameworkCore;
using psi25_project.Data;
using psi25_project.Models;
using psi25_project.Repositories.Interfaces;

namespace psi25_project.Repositories;

public class AchievementRepository : IAchievementRepository
{
    private readonly GeoHuntContext _context;

    public AchievementRepository(GeoHuntContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Achievement>> GetActiveByCodesAsync(IEnumerable<string> codes)
    {
        var codeList = codes.Distinct().ToList();
        if (codeList.Count == 0)
            return Array.Empty<Achievement>();

        return await _context.Achievements
            .Where(a => a.IsActive && codeList.Contains(a.Code))
            .ToListAsync();
    }

    public async Task<List<Achievement>> GetActiveAchievementsAsync()
    {
        return await _context.Achievements
            .Where(a => a.IsActive)
            .ToListAsync();
    }

    public async Task<List<UserAchievement>> GetAchievementsForUserAsync(Guid userId)
    {
        return await _context.UserAchievements
            .AsNoTracking()
            .Include(ua => ua.Achievement)
            .Where(ua => ua.UserId == userId)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<UserAchievement>> GetUnlockedAsync(Guid userId, IEnumerable<int> achievementIds)
    {
        var ids = achievementIds.Distinct().ToList();
        if (ids.Count == 0)
            return Array.Empty<UserAchievement>();

        return await _context.UserAchievements
            .Include(ua => ua.Achievement)
            .AsNoTracking()
            .Where(ua => ua.UserId == userId && ids.Contains(ua.AchievementId))
            .ToListAsync();
    }

    public async Task AddNewlyUnlockedAchievementsAsync(IEnumerable<UserAchievement> unlocks, CancellationToken ct = default)
    {
        var list = unlocks.ToList();
        if (list.Count == 0) return;

        await _context.UserAchievements.AddRangeAsync(list, ct);
        await _context.SaveChangesAsync(ct);
    }
}
