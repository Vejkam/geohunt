using psi25_project.Models;
using psi25_project.Models.Dtos;

namespace psi25_project.Services.Interfaces
{
    public interface IMultiplayerGameService
    {
        Task<MultiplayerGameDto> StartGameAsync(Guid roomId);
        Task<RoundResultDto> SubmitGuessAsync(Guid playerId, double latitude, double longitude);
        Task<MultiplayerGameDto> NextRoundAsync(Guid gameId);
        Task<MultiplayerGameDto> EndGameAsync(Guid gameId);
        Task<List<GameResultDto>> GetPastGamesForRoomAsync(Guid roomId);
        Task<MultiplayerGame?> GetCurrentGameForRoomAsync(Guid roomId);
        Task<MultiplayerPlayer?> GetCurrentGameForPlayerAsync(Guid playerId);
    }
}
