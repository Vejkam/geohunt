using psi25_project.Models;
using psi25_project.Models.Dtos;
using psi25_project.Repositories.Interfaces;
using psi25_project.Services.Interfaces;

namespace psi25_project.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;

        public GameService(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        private async Task<Game> GetGameOrThrowAsync(Guid gameId)
        {
            var game = await _gameRepository.GetByIdAsync(gameId);
            if (game == null)
                throw new KeyNotFoundException("Game not found");

            return game;
        }

        public async Task<GameResponseDto> StartGameAsync(CreateGameDto dto)
        {
            var game = new Game
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                User = null!,
                StartedAt = DateTime.UtcNow,
                FinishedAt = null,
                CurrentRound = 1,
                TotalRounds = dto.TotalRounds <= 0 ? 3 : dto.TotalRounds,
                TotalScore = 0
            };

            await _gameRepository.AddAsync(game);

            return ToDto(game);
        }

        public async Task<Game> FinishGameAsync(Guid gameId)
        {
            var game = await GetGameOrThrowAsync(gameId);

            if (game.FinishedAt != null)
                throw new InvalidOperationException("Game is already finished");

            game.FinishedAt = DateTime.UtcNow;
            await _gameRepository.UpdateAsync(game);

            return game;
        }

        public async Task<Game> UpdateScoreAsync(Guid gameId, int score)
        {
            var game = await GetGameOrThrowAsync(gameId);
            game.TotalScore += score;
            await _gameRepository.UpdateAsync(game);

            return game;
        }

        public async Task<int> GetTotalScoreAsync(Guid gameId)
        {
            var game = await GetGameOrThrowAsync(gameId);
            return game.TotalScore;
        }

        public async Task<GameResponseDto?> GetGameByIdAsync(Guid gameId)
        {
            var game = await _gameRepository.GetByIdAsync(gameId);
            return game == null ? null : ToDto(game);
        }

        private static GameResponseDto ToDto(Game game) =>
            new()
            {
                Id = game.Id,
                UserId = game.UserId,
                TotalScore = game.TotalScore,
                StartedAt = game.StartedAt,
                FinishedAt = game.FinishedAt,
                CurrentRound = game.CurrentRound,
                TotalRounds = game.TotalRounds
            };
    }
}