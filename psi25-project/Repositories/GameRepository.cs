using psi25_project.Data;
using psi25_project.Models;
using psi25_project.Repositories.Interfaces;

namespace psi25_project.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly GeoHuntContext _context;

        public GameRepository(GeoHuntContext context)
        {
            _context = context;
        }

        public async Task<Game?> GetByIdAsync(Guid gameId)
        {
            return await _context.Games.FindAsync(gameId);
        }

        public async Task AddAsync(Game game)
        {
            await _context.Games.AddAsync(game);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Game game)
        {
            _context.Games.Update(game);
            await _context.SaveChangesAsync();
        }
    }
}
