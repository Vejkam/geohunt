using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using psi25_project.Data;
using psi25_project.Models;
using psi25_project.Services.Interfaces;


namespace psi25_project.Hubs
{
    public class RoomHub : Hub
    {
        private readonly IRoomOnlineService _roomOnlineService;
        private readonly GeoHuntContext _context;

        public RoomHub(IRoomOnlineService roomOnlineService, GeoHuntContext context)
        {
            _roomOnlineService = roomOnlineService;
            _context = context;
        }

        public async Task JoinRoom(string roomCode, Guid playerId, string displayName)
        {
            var connectionId = Context.ConnectionId;
            
            var room = await _context.Rooms
                .Include(r => r.Players)
                .FirstOrDefaultAsync(r => r.RoomCode == roomCode);
                
            if (room == null)
            {
                throw new HubException("Room not found");
            }
            
            var roomId = room.Id.ToString();
            await Groups.AddToGroupAsync(connectionId, roomId);
            
            var existingPlayer = _roomOnlineService.GetOnlinePlayers(roomId)
                .FirstOrDefault(p => p.PlayerId == playerId);
            
            if (existingPlayer != null)
            {
                Console.WriteLine($"Player {playerId} already in room, removing old connection");
                _roomOnlineService.RemoveOnlinePlayer(roomId, existingPlayer.ConnectionId);
            }
            
            _roomOnlineService.AddOnlinePlayer(roomId, new PlayerOnline
            {
                PlayerId = playerId,
                DisplayName = displayName,
                ConnectionId = connectionId,
                IsReady = false
            });

            Console.WriteLine($"Player {displayName} joined room {roomCode}");

            var dbPlayers = room.Players.Select(p => new
            {
                id = p.Id.ToString(),
                userId = p.UserId.ToString(),
                displayName = p.DisplayName,
                isReady = p.IsReady
            }).ToList();

            await Clients.Group(roomId).SendAsync("PlayerListUpdated", dbPlayers);
        }

        public async Task LeaveRoom(string roomCode)
        {
            var connectionId = Context.ConnectionId;
            
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomCode == roomCode);
                
            if (room == null) return;
            
            var roomId = room.Id.ToString();
            
            var removedPlayer = _roomOnlineService.RemoveOnlinePlayer(roomId, connectionId);
            if (removedPlayer != null)
            {
                await Clients.Group(roomId).SendAsync("PlayerLeft", removedPlayer.PlayerId);
                
                var dbRoom = await _context.Rooms
                    .Include(r => r.Players)
                    .FirstOrDefaultAsync(r => r.Id == room.Id);
                    
                if (dbRoom != null)
                {
                    var dbPlayers = dbRoom.Players.Select(p => new
                    {
                        id = p.Id.ToString(),
                        userId = p.UserId.ToString(),
                        displayName = p.DisplayName,
                        isReady = p.IsReady
                    }).ToList();

                    await Clients.Group(roomId).SendAsync("PlayerListUpdated", dbPlayers);
                }
            }

            await Groups.RemoveFromGroupAsync(connectionId, roomId);
        }

        public async Task UpdateReadyState(string roomCode, Guid playerId, bool isReady)
        {
            var room = await _context.Rooms
                .Include(r => r.Players)
                .FirstOrDefaultAsync(r => r.RoomCode == roomCode);
                
            if (room == null) return;
            
            var roomId = room.Id.ToString();
            
            _roomOnlineService.UpdatePlayerState(roomId, playerId, isReady);
            
            var dbPlayers = room.Players.Select(p => new
            {
                id = p.Id.ToString(),
                userId = p.UserId.ToString(),
                displayName = p.DisplayName,
                isReady = p.IsReady
            }).ToList();

            await Clients.Group(roomId).SendAsync("PlayerListUpdated", dbPlayers);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            var roomIds = _roomOnlineService.GetRoomsForConnection(connectionId);
            
            foreach (var roomId in roomIds)
            {
                var removedPlayer = _roomOnlineService.RemoveOnlinePlayer(roomId, connectionId);
                if (removedPlayer != null)
                {
                    await Clients.Group(roomId).SendAsync("PlayerLeft", removedPlayer.PlayerId);
                    
                    var room = await _context.Rooms
                        .Include(r => r.Players)
                        .FirstOrDefaultAsync(r => r.Id.ToString() == roomId);
                        
                    if (room != null)
                    {
                        var dbPlayers = room.Players.Select(p => new
                        {
                            id = p.Id.ToString(),
                            userId = p.UserId.ToString(),
                            displayName = p.DisplayName,
                            isReady = p.IsReady
                        }).ToList();

                        await Clients.Group(roomId).SendAsync("PlayerListUpdated", dbPlayers);
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}