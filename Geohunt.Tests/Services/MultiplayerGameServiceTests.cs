using Moq;
using psi25_project.Models;
using psi25_project.Repositories.Interfaces;
using psi25_project.Services;
using psi25_project.Services.Interfaces;
using System.Dynamic;

namespace Geohunt.Tests.Services;

public class MultiplayerGameServiceTests
{
    private readonly Mock<IMultiplayerGameRepository> _mockGameRepository;
    private readonly Mock<IPlayerRepository> _mockPlayerRepository;
    private readonly Mock<IRoomRepository> _mockRoomRepository;
    private readonly Mock<IGeocodingService> _mockGeocodingService;

    private readonly MultiplayerGameService _service;

    public MultiplayerGameServiceTests()
    {
        _mockGameRepository = new Mock<IMultiplayerGameRepository>();
        _mockPlayerRepository = new Mock<IPlayerRepository>();
        _mockRoomRepository = new Mock<IRoomRepository>();
        _mockGeocodingService = new Mock<IGeocodingService>();

        _service = new MultiplayerGameService(
            _mockGameRepository.Object,
            _mockPlayerRepository.Object,
            _mockRoomRepository.Object,
            _mockGeocodingService.Object);
    }

    #region StartGameAsync

    [Fact]
    public async Task StartGameAsync_AllPlayersReady_CreatesGameAndUpdatesRoom()
    {
        var roomId = Guid.NewGuid();
        var player1 = new Player { Id = Guid.NewGuid(), DisplayName = "Alice", IsReady = true };
        var player2 = new Player { Id = Guid.NewGuid(), DisplayName = "Bob", IsReady = true };

        var room = new Room
        {
            Id = roomId,
            TotalRounds = 3,
            Status = RoomStatus.Lobby,
            Players = new List<Player> { player1, player2 }
        };

        _mockRoomRepository.Setup(r => r.GetRoomByIdAsync(roomId)).ReturnsAsync(room);

        dynamic coords = new ExpandoObject();
        coords.lat = 10.0;
        coords.lng = 20.0;
        dynamic geoResult = new ExpandoObject();
        geoResult.modifiedCoordinates = coords;

        _mockGeocodingService.Setup(g => g.GetValidCoordinatesAsync()).ReturnsAsync((true, geoResult));

        MultiplayerGame? capturedGame = null;
        _mockGameRepository.Setup(r => r.AddAsync(It.IsAny<MultiplayerGame>())).Callback<MultiplayerGame>(g =>
        {
            foreach (var mp in g.Players)
                mp.Player = room.Players.First(p => p.Id == mp.PlayerId);
            capturedGame = g;
        }).Returns(Task.CompletedTask);

        _mockRoomRepository.Setup(r => r.UpdateRoomAsync(room)).Returns(Task.CompletedTask);

        var result = await _service.StartGameAsync(roomId);

        Assert.NotNull(capturedGame);
        Assert.Equal(roomId, capturedGame!.RoomId);
        Assert.Equal(1, capturedGame.CurrentRound);
        Assert.Equal(room.TotalRounds, capturedGame.TotalRounds);
        Assert.Equal(GameState.InProgress, capturedGame.State);
        Assert.Equal(10.0, capturedGame.RoundLatitude);
        Assert.Equal(20.0, capturedGame.RoundLongitude);
        Assert.Equal(2, capturedGame.Players.Count);
        Assert.Equal(RoomStatus.InGame, room.Status);
        Assert.Equal(capturedGame.Id, result.GameId);
        Assert.Equal(roomId, result.RoomId);
    }

    [Fact]
    public async Task StartGameAsync_RoomNotFound_ThrowsKeyNotFoundException()
    {
        var roomId = Guid.NewGuid();
        _mockRoomRepository.Setup(r => r.GetRoomByIdAsync(roomId)).ReturnsAsync((Room?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.StartGameAsync(roomId));
    }

    [Fact]
    public async Task StartGameAsync_NotAllPlayersReady_ThrowsInvalidOperationException()
    {
        var room = new Room
        {
            Id = Guid.NewGuid(),
            Players = new List<Player>
            {
                new() { IsReady = true },
                new() { IsReady = false }
            }
        };

        _mockRoomRepository.Setup(r => r.GetRoomByIdAsync(room.Id)).ReturnsAsync(room);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.StartGameAsync(room.Id));
    }

    [Fact]
    public async Task StartGameAsync_GeocodingFails_ThrowsInvalidOperationException()
    {
        var room = new Room
        {
            Id = Guid.NewGuid(),
            TotalRounds = 1,
            Players = new List<Player> { new() { IsReady = true, Id = Guid.NewGuid(), DisplayName = "Alice" } }
        };

        _mockRoomRepository.Setup(r => r.GetRoomByIdAsync(room.Id)).ReturnsAsync(room);
        _mockGeocodingService.Setup(g => g.GetValidCoordinatesAsync()).ReturnsAsync((false, null));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.StartGameAsync(room.Id));
    }

    #endregion

    #region SubmitGuessAsync

    [Fact]
    public async Task SubmitGuessAsync_ValidGuess_UpdatesPlayerAndReturnsResult()
    {
        var roomId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var player = new Player { Id = playerId, RoomId = roomId };

        var game = new MultiplayerGame
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            RoundLatitude = 10,
            RoundLongitude = 20,
            Players = new List<MultiplayerPlayer>
            {
                new() { PlayerId = playerId, Player = player, Finished = false, Score = 0 }
            }
        };

        _mockPlayerRepository.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);
        _mockGameRepository.Setup(r => r.GetByRoomIdAsync(roomId)).ReturnsAsync(game);
        _mockGameRepository.Setup(r => r.UpdateAsync(game)).Returns(Task.CompletedTask);

        var result = await _service.SubmitGuessAsync(playerId, 11, 21);

        var mp = game.Players.Single();
        Assert.True(mp.Finished);
        Assert.True(mp.Score > 0);
        Assert.Equal(playerId, result.PlayerId);
        Assert.True(result.RoundFinished);
    }

    [Fact]
    public async Task SubmitGuessAsync_PlayerNotFound_ThrowsKeyNotFoundException()
    {
        var playerId = Guid.NewGuid();
        _mockPlayerRepository.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync((Player?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.SubmitGuessAsync(playerId, 0, 0));
    }

    [Fact]
    public async Task SubmitGuessAsync_GameNotFound_ThrowsInvalidOperationException()
    {
        var playerId = Guid.NewGuid();
        var player = new Player { Id = playerId, RoomId = Guid.NewGuid() };
        _mockPlayerRepository.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);
        _mockGameRepository.Setup(r => r.GetByRoomIdAsync(player.RoomId.Value)).ReturnsAsync((MultiplayerGame?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SubmitGuessAsync(playerId, 0, 0));
    }

    [Fact]
    public async Task SubmitGuessAsync_PlayerNotInGame_ThrowsKeyNotFoundException()
    {
        var roomId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var player = new Player { Id = playerId, RoomId = roomId };

        var game = new MultiplayerGame
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            Players = new List<MultiplayerPlayer>()
        };

        _mockPlayerRepository.Setup(r => r.GetPlayerByIdAsync(playerId)).ReturnsAsync(player);
        _mockGameRepository.Setup(r => r.GetByRoomIdAsync(roomId)).ReturnsAsync(game);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.SubmitGuessAsync(playerId, 0, 0));
    }

    #endregion

    #region NextRoundAsync

    [Fact]
    public async Task NextRoundAsync_ValidGame_AdvancesRoundAndResetsPlayers()
    {
        var player = new Player { Id = Guid.NewGuid(), DisplayName = "Test" };
        var game = new MultiplayerGame
        {
            Id = Guid.NewGuid(),
            CurrentRound = 1,
            TotalRounds = 3,
            Players = new List<MultiplayerPlayer>
            {
                new() { PlayerId = player.Id, Player = player, Finished = true, LastGuessLatitude = 1 }
            }
        };

        _mockGameRepository.Setup(r => r.GetByIdAsync(game.Id)).ReturnsAsync(game);

        dynamic coords = new ExpandoObject();
        coords.lat = 30.0;
        coords.lng = 40.0;
        dynamic geoResult = new ExpandoObject();
        geoResult.modifiedCoordinates = coords;

        _mockGeocodingService.Setup(g => g.GetValidCoordinatesAsync()).ReturnsAsync((true, geoResult));

        var result = await _service.NextRoundAsync(game.Id);

        Assert.Equal(2, game.CurrentRound);
        Assert.Equal(30.0, game.RoundLatitude);
        Assert.Equal(40.0, game.RoundLongitude);
        Assert.False(game.Players.Single().Finished);
    }

    [Fact]
    public async Task NextRoundAsync_GameNotFound_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _mockGameRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((MultiplayerGame?)null);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.NextRoundAsync(id));
    }

    [Fact]
    public async Task NextRoundAsync_AllRoundsCompleted_ThrowsInvalidOperationException()
    {
        var game = new MultiplayerGame { Id = Guid.NewGuid(), CurrentRound = 3, TotalRounds = 3 };
        _mockGameRepository.Setup(r => r.GetByIdAsync(game.Id)).ReturnsAsync(game);
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.NextRoundAsync(game.Id));
    }

    [Fact]
    public async Task NextRoundAsync_GeocodingFails_ThrowsInvalidOperationException()
    {
        var game = new MultiplayerGame { Id = Guid.NewGuid(), CurrentRound = 1, TotalRounds = 3, Players = new List<MultiplayerPlayer>() };
        _mockGameRepository.Setup(r => r.GetByIdAsync(game.Id)).ReturnsAsync(game);
        _mockGeocodingService.Setup(g => g.GetValidCoordinatesAsync()).ReturnsAsync((false, null));
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.NextRoundAsync(game.Id));
    }

    #endregion

    #region EndGameAsync

    [Fact]
    public async Task EndGameAsync_EndsGame_ResetsRoomAndPlayers()
    {
        var roomId = Guid.NewGuid();
        var player = new Player { Id = Guid.NewGuid(), DisplayName = "P1", IsReady = true };
        var game = new MultiplayerGame { Id = Guid.NewGuid(), RoomId = roomId, State = GameState.InProgress };
        var room = new Room { Id = roomId, Status = RoomStatus.InGame, Players = new List<Player> { player } };

        _mockGameRepository.Setup(r => r.GetByIdAsync(game.Id)).ReturnsAsync(game);
        _mockRoomRepository.Setup(r => r.GetRoomByIdAsync(roomId)).ReturnsAsync(room);

        var result = await _service.EndGameAsync(game.Id);

        Assert.Equal(GameState.Finished, game.State);
        Assert.Equal(RoomStatus.Lobby, room.Status);
        Assert.False(room.Players[0].IsReady);
        Assert.Equal(game.Id, result.GameId);
    }

    [Fact]
    public async Task EndGameAsync_GameNotFound_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        _mockGameRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((MultiplayerGame?)null);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.EndGameAsync(id));
    }

    #endregion

    #region GetPastGamesForRoomAsync

    [Fact]
    public async Task GetPastGamesForRoomAsync_ReturnsGameResults()
    {
        var roomId = Guid.NewGuid();
        var player = new Player { Id = Guid.NewGuid(), DisplayName = "Alice" };
        var game = new MultiplayerGame
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            StartedAt = DateTime.UtcNow,
            FinishedAt = DateTime.UtcNow.AddMinutes(5),
            Players = new List<MultiplayerPlayer>
            {
                new() { PlayerId = player.Id, Player = player, Score = 100 }
            }
        };

        _mockGameRepository.Setup(r => r.GetPastGamesForRoomAsync(roomId)).ReturnsAsync(new List<MultiplayerGame> { game });

        var result = await _service.GetPastGamesForRoomAsync(roomId);

        Assert.Single(result);
        Assert.Single(result[0].PlayerScores);
        Assert.Equal(100, result[0].PlayerScores[0].Score);
    }

    #endregion

    #region GetCurrentGameForRoomAsync / GetCurrentGameForPlayerAsync

    [Fact]
    public async Task GetCurrentGameForRoomAsync_ReturnsGame()
    {
        var roomId = Guid.NewGuid();
        var game = new MultiplayerGame { Id = Guid.NewGuid(), RoomId = roomId };
        _mockGameRepository.Setup(r => r.GetByRoomIdAsync(roomId)).ReturnsAsync(game);

        var result = await _service.GetCurrentGameForRoomAsync(roomId);
        Assert.Equal(game, result);
    }

    [Fact]
    public async Task GetCurrentGameForPlayerAsync_ReturnsNullIfNoRoom()
    {
        var player = new Player { Id = Guid.NewGuid(), RoomId = null };
        _mockPlayerRepository.Setup(r => r.GetPlayerByIdAsync(player.Id)).ReturnsAsync(player);

        var result = await _service.GetCurrentGameForPlayerAsync(player.Id);
        Assert.Null(result);
    }

    #endregion
}
