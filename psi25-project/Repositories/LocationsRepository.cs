using Microsoft.EntityFrameworkCore;
using psi25_project.Data;
using psi25_project.Models;
using psi25_project.Repositories.Interfaces;

namespace psi25_project.Repositories
{
    public class LocationRepository : ILocationRepository
    {
        private readonly GeoHuntContext _context;

        public LocationRepository(GeoHuntContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Location>> GetAllAsync()
        {
            return await _context.Locations.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<object>> GetRecentAsync(DateTime cutoffUtc)
        {
            return await _context.Locations
                .AsNoTracking()
                .Where(l => l.LastPlayedAt > cutoffUtc)
                .OrderByDescending(l => l.LastPlayedAt)
                .Take(20)
                .Select(l => new
                {
                    l.Id,
                    l.Latitude,
                    l.Longitude,
                    l.panoId,
                    l.LastPlayedAt
                })
                .ToListAsync();
        }

        public async Task<Location?> GetByIdAsync(int id)
        {
            return await _context.Locations.FindAsync(id);
        }

        public async Task AddAsync(Location location)
        {
            _context.Locations.Add(location);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Location location)
        {
            _context.Locations.Update(location);
            await _context.SaveChangesAsync();
        }
    }
}
