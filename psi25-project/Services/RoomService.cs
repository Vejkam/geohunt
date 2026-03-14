using psi25_project.Repositories.Interfaces;
using psi25_project.Models;
using psi25_project.Models.Dtos;
using psi25_project.Services.Interfaces;

namespace psi25_project.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _rooms;
        private readonly IPlayerRepository _players;

        public RoomService(IRoomRepository rooms, IPlayerRepository players)
        {
            _rooms = rooms;
            _players = players;
        }

        public async Task<RoomDto> CreateRoomAsync(RoomCreateDto dto)
        {
            var room = new Room
            {
                RoomCode = GenerateCode(),
                CreatedAt = DateTime.UtcNow,
                TotalRounds = dto.TotalRounds,
                CurrentRounds = 1
            };

            var createdRoom = await _rooms.CreateRoomAsync(room);

            return MapToRoomDto(createdRoom);
        }

        public async Task<PlayerDto?> JoinRoomAsync(JoinRoomDto dto)
        {
            var room = await _rooms.GetRoomByCodeAsync(dto.RoomCode);
            if (room == null) return null;

            var existingPlayer = await _players.GetPlayerByUserAndRoomAsync(dto.UserId, room.Id);
            if (existingPlayer != null)
                return MapToPlayerDto(existingPlayer);

            var player = new Player
            {
                UserId = dto.UserId,
                RoomId = room.Id,
                DisplayName = dto.DisplayName,
                Score = 0,
                IsReady = false
            };

            await _players.AddPlayerAsync(player);
            return MapToPlayerDto(player);
        }

        public async Task<List<PlayerDto>> GetPlayersInRoomAsync(string roomCode)
        {
            var room = await _rooms.GetRoomWithPlayersAsync(roomCode);
            return room?.Players.Select(MapToPlayerDto).ToList() ?? new List<PlayerDto>();
        }

        public async Task<PlayerDto?> SetReadyAsync(Guid playerId)
        {
            var player = await _players.GetPlayerByIdAsync(playerId);
            if (player == null) return null;

            player.IsReady = true;
            await _players.UpdatePlayerAsync(player);

            return MapToPlayerDto(player);
        }

        public async Task<PlayerDto?> ToggleReadyAsync(Guid playerId)
        {
            var player = await _players.GetPlayerByIdAsync(playerId);
            if (player == null) return null;

            player.IsReady = !player.IsReady;
            await _players.UpdatePlayerAsync(player);

            return MapToPlayerDto(player);
        }

        public async Task<bool> LeaveRoomAsync(Guid playerId)
        {
            var player = await _players.GetPlayerByIdAsync(playerId);
            if (player == null || !player.RoomId.HasValue) return false;

            var roomId = player.RoomId.Value;

            await _players.RemovePlayerAsync(player.Id);

            var playersInRoom = await _players.GetPlayersByRoomIdAsync(roomId);
            if (!playersInRoom.Any())
            {
                await _rooms.DeleteRoomAsync(roomId);
            }

            return true;
        }

        private string GenerateCode() => Guid.NewGuid().ToString("N")[..5].ToUpper();

        private RoomDto MapToRoomDto(Room room) => new RoomDto
        {
            Id = room.Id,
            RoomCode = room.RoomCode,
            TotalRounds = room.TotalRounds,
            CurrentRounds = room.CurrentRounds
        };

        private PlayerDto MapToPlayerDto(Player player) => new PlayerDto
        {
            Id = player.Id,
            UserId = player.UserId,
            DisplayName = player.DisplayName,
            IsReady = player.IsReady
        };
    }
}
