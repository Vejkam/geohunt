using Microsoft.EntityFrameworkCore;
using psi25_project.Data;
using psi25_project.Models;
using psi25_project.Repositories.Interfaces;

namespace psi25_project.Repositories;

public class UserStatsRepository : IUserStatsRepository
{
    private readonly GeoHuntContext _context;

    public UserStatsRepository(GeoHuntContext context)
    {
        _context = context;
    }

        public async Task<UserStats> GetOrCreateAsync(Guid userId)
    {
        var stats = await _context.UserStats
            .SingleOrDefaultAsync(s => s.UserId == userId);

        if (stats != null)
            return stats;

        stats = new UserStats
        {
            UserId = userId,
            TotalGames = 0,
            TotalGuesses = 0,
            BestGameScore = 0,
            CurrentStreakDays = 0,
            LastPlayedDateUtc = null
        };

        _context.UserStats.Add(stats);
        await _context.SaveChangesAsync();

        return stats;
    }

    public async Task UpdateAsync(UserStats stats)
    {
        _context.UserStats.Update(stats);
        await _context.SaveChangesAsync();
    }
}