using psi25_project.Models;

namespace psi25_project.Repositories.Interfaces;

public interface IUserStatsRepository
{
    Task<UserStats> GetOrCreateAsync(Guid userId);
    Task UpdateAsync(UserStats stats);
}