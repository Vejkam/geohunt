using psi25_project.Models;

namespace psi25_project.Repositories.Interfaces
{
    public interface ILocationRepository
    {
        Task<IEnumerable<Location>> GetAllAsync();
        Task<IEnumerable<object>> GetRecentAsync(DateTime cutoffUtc);
        Task<Location?> GetByIdAsync(int id);
        Task AddAsync(Location location);
        Task UpdateAsync(Location location);
    }
}
