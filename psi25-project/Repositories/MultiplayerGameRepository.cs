using Microsoft.EntityFrameworkCore;
using psi25_project.Data;
using psi25_project.Models;
using psi25_project.Repositories.Interfaces;

namespace psi25_project.Repositories
{
    public class MultiplayerGameRepository : IMultiplayerGameRepository
    {
        private readonly GeoHuntContext _context;

        public MultiplayerGameRepository(GeoHuntContext context)
        {
            _context = context;
        }

        public async Task<MultiplayerGame?> GetByIdAsync(Guid gameId)
        {
            return await _context.MultiplayerGames
                .Include(g => g.Players)
                .ThenInclude(mp => mp.Player)
                .FirstOrDefaultAsync(g => g.Id == gameId);
        }

        public async Task<MultiplayerGame?> GetByRoomIdAsync(Guid roomId)
        {
            return await _context.MultiplayerGames
                .Include(g => g.Players)
                .ThenInclude(mp => mp.Player)
                .FirstOrDefaultAsync(g => g.RoomId == roomId && g.State != Models.GameState.Finished);
        }

        public async Task<List<MultiplayerGame>> GetPastGamesForRoomAsync(Guid roomId)
        {
            return await _context.MultiplayerGames
                .Where(g => g.RoomId == roomId && g.State == Models.GameState.Finished)
                .Include(g => g.Players)
                .ThenInclude(mp => mp.Player)
                .OrderByDescending(g => g.StartedAt)
                .ToListAsync();
        }

        public async Task AddAsync(MultiplayerGame game)
        {
            await _context.MultiplayerGames.AddAsync(game);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(MultiplayerGame game)
        {
            _context.MultiplayerGames.Update(game);
            await _context.SaveChangesAsync();
        }
    }
}
