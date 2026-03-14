using psi25_project.Models.Dtos;

namespace psi25_project.Services.Interfaces
{
    public interface ILeaderboardService
    {
        Task<List<LeaderboardEntry>> GetTopLeaderboardAsync(int top = 20);
        Task<List<LeaderboardEntry>> GetTopPlayersAsync(int top = 20);
    }
}
