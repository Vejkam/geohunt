using psi25_project.Models;

namespace psi25_project.Services.Interfaces
{
    public interface IRoomOnlineService
    {
        void AddOnlinePlayer(string roomId, PlayerOnline player);

        PlayerOnline? RemoveOnlinePlayer(string roomId, string connectionId);

        void UpdatePlayerState(string roomId, Guid playerId, bool isReady);

        List<PlayerOnline> GetOnlinePlayers(string roomId);

        List<string> GetAllRooms();

        List<string> GetRoomsForConnection(string connectionId);
    } 
}
