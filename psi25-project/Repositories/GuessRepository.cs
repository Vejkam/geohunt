using Microsoft.EntityFrameworkCore;
using psi25_project.Data;
using psi25_project.Models;
using psi25_project.Repositories.Interfaces;

namespace psi25_project.Repositories
{
    public class GuessRepository : IGuessRepository
    {
        private readonly GeoHuntContext _context;

        public GuessRepository(GeoHuntContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Guess guess)
        {
            _context.Guesses.Add(guess);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Guess>> GetGuessesByGameAsync(Guid gameId)
        {
            return await _context.Guesses
                .Include(g => g.Location)
                .Where(g => g.GameId == gameId)
                .ToListAsync();
        }

        public async Task<int> CountForUserAsync(Guid userId)
        {
            return await _context.Guesses
                .CountAsync(g => g.Game.UserId == userId);
        }
    }
}
