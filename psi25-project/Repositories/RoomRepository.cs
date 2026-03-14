using Microsoft.EntityFrameworkCore;
using psi25_project.Data;
using psi25_project.Models;
using psi25_project.Repositories.Interfaces;

namespace psi25_project.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly GeoHuntContext _context;

        public RoomRepository(GeoHuntContext context)
        {
            _context = context;
        }

        public async Task<Room> CreateRoomAsync(Room room)
        {
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
            return room;
        }

        public async Task<Room?> GetRoomByCodeAsync(string roomCode)
        {
            return await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomCode == roomCode);
        }

        public async Task<Room?> GetRoomWithPlayersAsync(string roomCode)
        {
            return await _context.Rooms
                .Include(r => r.Players)
                .FirstOrDefaultAsync(r => r.RoomCode == roomCode);
        }

        public async Task<Room?> DeleteRoomAsync(Guid roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null) return null;

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return room;
        }

        public async Task<Room?> GetRoomByIdAsync(Guid roomId)
        {
            return await _context.Rooms
                .Include(r => r.Players)
                .FirstOrDefaultAsync(r => r.Id == roomId);
        }

        public async Task UpdateRoomAsync(Room room)
        {
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();
        }

    }
}
