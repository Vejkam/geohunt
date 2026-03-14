using Microsoft.EntityFrameworkCore;
using psi25_project.Data;
using psi25_project.Models.Dtos;
using psi25_project.Repositories.Interfaces;

namespace psi25_project.Repositories
{
    public class LeaderboardRepository : ILeaderboardRepository
    {
        private readonly GeoHuntContext _context;

        public LeaderboardRepository(GeoHuntContext context)
        {
            _context = context;
        }

        public async Task<List<LeaderboardEntry>> GetTopGuessesAsync(int top = 20)
        {
            var entries = await _context.Guesses
                .Include(g => g.Game)
                .ThenInclude(game => game.User)
                .Select(g => new LeaderboardEntry
                {
                    Id = g.Id,
                    DistanceKm = g.DistanceKm,
                    GuessedAt = g.GuessedAt,
                    TotalScore = g.Score,
                    UserId = g.Game.User.Id,
                    Username = g.Game.User.UserName
                })
                .OrderByDescending(x => x.TotalScore)
                .Take(top)
                .ToListAsync();

            entries.Sort();
            for (int i = 0; i < entries.Count; i++)
                entries[i].Rank = i + 1;

            return entries;
        }

        public async Task<List<LeaderboardEntry>> GetTopPlayersAsync(int top = 20)
        {
            var entries = await _context.Guesses
                .Include(g => g.Game)
                .ThenInclude(game => game.User)
                .GroupBy(g => new
                {
                    g.Game.UserId,
                    g.Game.User.UserName
                })
                .Select(group => new LeaderboardEntry
                {
                    UserId = group.Key.UserId,
                    Username = group.Key.UserName,
                    TotalScore = group.Max(g => g.Score),
                    DistanceKm = group
                        .OrderByDescending(g => g.Score)
                        .ThenBy(g => g.DistanceKm)
                        .Select(g => g.DistanceKm)
                        .FirstOrDefault(),
                    GuessedAt = group
                        .OrderByDescending(g => g.Score)
                        .ThenBy(g => g.DistanceKm)
                        .Select(g => g.GuessedAt)
                        .FirstOrDefault()
                })
                .OrderByDescending(x => x.TotalScore)
                .Take(top)
                .ToListAsync();

            for (int i = 0; i < entries.Count; i++)
                entries[i].Rank = i + 1;

            return entries;
        }
    }
}
