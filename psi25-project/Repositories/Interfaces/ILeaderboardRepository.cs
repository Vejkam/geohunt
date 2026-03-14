using psi25_project.Models.Dtos;

namespace psi25_project.Repositories.Interfaces
{
    public interface ILeaderboardRepository
    {
        Task<List<LeaderboardEntry>> GetTopGuessesAsync(int top = 20);
        Task<List<LeaderboardEntry>> GetTopPlayersAsync(int top = 20);
    }
}
