using psi25_project.Models.Dtos;

namespace psi25_project.Services.Interfaces
{
    public interface IRoomService
    {
        Task<RoomDto> CreateRoomAsync(RoomCreateDto dto);
        Task<PlayerDto?> JoinRoomAsync(JoinRoomDto dto);
        Task<List<PlayerDto>> GetPlayersInRoomAsync(string roomCode);
        Task<PlayerDto?> SetReadyAsync(Guid playerId);
        Task<PlayerDto?> ToggleReadyAsync(Guid playerId);
        Task<bool> LeaveRoomAsync(Guid playerId);
    }
}
