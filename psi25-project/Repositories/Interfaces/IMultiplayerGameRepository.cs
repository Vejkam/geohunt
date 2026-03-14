using psi25_project.Models;

namespace psi25_project.Repositories.Interfaces
{
    public interface IMultiplayerGameRepository
    {
        Task<MultiplayerGame?> GetByIdAsync(Guid gameId);
        Task<MultiplayerGame?> GetByRoomIdAsync(Guid roomId);
        Task<List<MultiplayerGame>> GetPastGamesForRoomAsync(Guid roomId);
        Task AddAsync(MultiplayerGame game);
        Task UpdateAsync(MultiplayerGame game);
    }
}
