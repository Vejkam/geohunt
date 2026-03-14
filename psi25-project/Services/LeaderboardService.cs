using psi25_project.Models.Dtos;
using psi25_project.Repositories.Interfaces;
using psi25_project.Services.Interfaces;

namespace psi25_project.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly ILeaderboardRepository _repository;

        public LeaderboardService(ILeaderboardRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<LeaderboardEntry>> GetTopLeaderboardAsync(int top = 20)
        {
            return await _repository.GetTopGuessesAsync(top);
        }

        public async Task<List<LeaderboardEntry>> GetTopPlayersAsync(int top = 20)
        {
            return await _repository.GetTopPlayersAsync(top);
        }
    }
}
