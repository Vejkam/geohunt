using psi25_project.Models;

namespace psi25_project.Repositories.Interfaces
{
    public interface IGuessRepository
    {
        Task AddAsync(Guess guess);
        Task<IEnumerable<Guess>> GetGuessesByGameAsync(Guid gameId);
        Task<int> CountForUserAsync(Guid userId);
    }
}

