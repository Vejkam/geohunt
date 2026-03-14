using psi25_project.Models;

namespace psi25_project.Repositories.Interfaces
{
    public interface IGameRepository
    {
        Task<Game?> GetByIdAsync(Guid gameId);
        Task AddAsync(Game game);
        Task UpdateAsync(Game game);
    }
}
