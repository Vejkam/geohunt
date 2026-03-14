using psi25_project.Models.Dtos;

namespace psi25_project.Services.Interfaces
{
    public interface IGuessService
    {
        Task<(GuessResponseDto guess, bool finished, int currentRound, int totalScore, IReadOnlyList<AchievementUnlockDto> newAchievements)> CreateGuessAsync(CreateGuessDto dto);
        Task<List<GuessResponseDto>> GetGuessesForGameAsync(Guid gameId);
    }
}
