using psi25_project.Models;
using psi25_project.Models.Dtos;

namespace psi25_project.Services
{
    public interface ILocationService
    {
        Task<IEnumerable<Location>> GetAllLocationsAsync();
        Task<IEnumerable<object>> GetRecentLocationsAsync();
        Task<Location> CreateLocationAsync(LocationDto dto);
        Task<(bool success, object result, string? message)> UpdateLastPlayedAsync(int id);
    }
}
