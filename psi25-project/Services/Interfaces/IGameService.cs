using psi25_project.Models;
using psi25_project.Models.Dtos;


namespace psi25_project.Services.Interfaces
{
    public interface IGameService
    {
        Task<GameResponseDto> StartGameAsync(CreateGameDto dto);
        Task<Game> FinishGameAsync(Guid gameId);
        Task<Game> UpdateScoreAsync(Guid gameId, int score);
        Task<int> GetTotalScoreAsync(Guid gameId);
        Task<GameResponseDto?> GetGameByIdAsync(Guid gameId);
    }
}
