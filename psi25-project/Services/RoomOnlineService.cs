using psi25_project.Models;
using System.Collections.Concurrent;
using psi25_project.Services.Interfaces;

namespace psi25_project.Services
{
    public class RoomOnlineService : IRoomOnlineService
    {
        private readonly ConcurrentDictionary<string, List<PlayerOnline>> _roomsOnline = new();

        public void AddOnlinePlayer(string roomId, PlayerOnline player)
        {
            var list = _roomsOnline.GetOrAdd(roomId, _ => new List<PlayerOnline>());
            lock (list)
            {
                if (!list.Any(p => p.ConnectionId == player.ConnectionId))
                {
                    list.Add(player);
                }
            }
        }

        public PlayerOnline? RemoveOnlinePlayer(string roomId, string connectionId)
        {
            if (_roomsOnline.TryGetValue(roomId, out var list))
            {
                lock (list)
                {
                    var player = list.FirstOrDefault(p => p.ConnectionId == connectionId);
                    if (player != null)
                    {
                        list.Remove(player);
                        if (list.Count == 0)
                            _roomsOnline.TryRemove(roomId, out _);
                        return player;
                    }
                }
            }
            return null;
        }

        public void UpdatePlayerState(string roomId, Guid playerId, bool isReady)
        {
            if (_roomsOnline.TryGetValue(roomId, out var list))
            {
                lock (list)
                {
                    var player = list.FirstOrDefault(p => p.PlayerId == playerId);
                    if (player != null)
                        player.IsReady = isReady;
                }
            }
        }

        public List<PlayerOnline> GetOnlinePlayers(string roomId)
        {
            if (_roomsOnline.TryGetValue(roomId, out var list))
            {
                lock (list)
                {
                    return list.Select(p => new PlayerOnline
                    {
                        PlayerId = p.PlayerId,
                        DisplayName = p.DisplayName,
                        ConnectionId = p.ConnectionId,
                        IsReady = p.IsReady
                    }).ToList();
                }
            }
            return new List<PlayerOnline>();
        }

        public List<string> GetAllRooms() => _roomsOnline.Keys.ToList();

        public List<string> GetRoomsForConnection(string connectionId)
        {
            var rooms = new List<string>();
            foreach (var kv in _roomsOnline)
            {
                lock (kv.Value)
                {
                    if (kv.Value.Any(p => p.ConnectionId == connectionId))
                        rooms.Add(kv.Key);
                }
            }
            return rooms;
        }
    }
}
