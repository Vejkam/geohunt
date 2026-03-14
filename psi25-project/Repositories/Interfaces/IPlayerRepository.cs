using psi25_project.Models;

namespace psi25_project.Repositories.Interfaces
{
    public interface IPlayerRepository
    {
        Task AddPlayerAsync(Player player);
        Task<Player?> GetPlayerByUserAndRoomAsync(Guid userId, Guid roomId);
        Task<Player?> GetPlayerByIdAsync(Guid playerId);
        Task UpdatePlayerAsync(Player player);
        Task<List<Player>> GetPlayersByRoomIdAsync(Guid roomId);
        Task<Player?> RemovePlayerAsync(Guid playerId);
        Task RemovePlayerAsync(Player player);
    }
}
