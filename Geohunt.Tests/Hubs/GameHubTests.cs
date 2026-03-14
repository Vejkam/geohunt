using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using psi25_project.Data;
using psi25_project.Hubs;
using psi25_project.Models;
using psi25_project.Models.Dtos;
using psi25_project.Services.Interfaces;

namespace Geohunt.Tests.Hubs;

public sealed class GameHubTests : IDisposable
{
    private readonly Mock<IMultiplayerGameService> _mockGameService = new();
    private readonly Mock<IHubCallerClients> _mockClients = new();
    private readonly Mock<IClientProxy> _mockGroupProxy = new();
    private readonly Mock<ISingleClientProxy> _mockCallerProxy = new();
    private readonly Mock<IGroupManager> _mockGroups = new();
    private readonly Mock<HubCallerContext> _mockContext = new();

    private readonly GeoHuntContext _context;
    private readonly GameHub _hub;

    public GameHubTests()
    {
        var options = new DbContextOptionsBuilder<GeoHuntContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new GeoHuntContext(options);

        _mockClients.Setup(c => c.Group(It.IsAny<string>()))
                    .Returns(_mockGroupProxy.Object);

        _mockClients.SetupGet(c => c.Caller)
                    .Returns(_mockCallerProxy.Object);

        _hub = new GameHub(_mockGameService.Object, _context)
        {
            Clients = _mockClients.Object,
            Groups = _mockGroups.Object,
            Context = _mockContext.Object
        };
    }

    public void Dispose() => _context.Dispose();

    private async Task<(Guid roomId, string roomCode)> SeedRoomAsync(string roomCode = "TEST123")
    {
        var roomId = Guid.NewGuid();
        _context.Rooms.Add(new Room
        {
            Id = roomId,
            RoomCode = roomCode,
            RowVersion = new byte[] { 1 }
        });
        await _context.SaveChangesAsync();
        return (roomId, roomCode);
    }

    private static MultiplayerGame MakeGame(
        Guid gameId,
        Guid roomId,
        int currentRound,
        int totalRounds,
        double? roundLat = null,
        double? roundLng = null,
        List<MultiplayerPlayer>? players = null)
    {
        var game = new MultiplayerGame
        {
            Id = gameId,
            RoomId = roomId,
            CurrentRound = currentRound,
            TotalRounds = totalRounds,
            RoundLatitude = roundLat,
            RoundLongitude = roundLng,
            Players = players ?? new List<MultiplayerPlayer>()
        };

        foreach (var p in game.Players)
            p.Game = game;

        return game;
    }

    [Fact]
    public async Task JoinGameRoom_ValidRoom_AddsToGroup()
    {
        var (roomId, roomCode) = await SeedRoomAsync("TEST123");
        var connectionId = "conn-123";
        _mockContext.SetupGet(c => c.ConnectionId).Returns(connectionId);

        await _hub.JoinGameRoom(roomCode);

        _mockGroups.Verify(g =>
                g.AddToGroupAsync(connectionId, roomId.ToString(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task JoinGameRoom_InvalidRoomCode_ThrowsHubException()
    {
        var ex = await Assert.ThrowsAsync<HubException>(() => _hub.JoinGameRoom("INVALID"));
        Assert.Equal("Room not found", ex.Message);
    }

    [Fact]
    public async Task StartGame_ValidRoom_BroadcastsGameStarted_RoundStarted_GameStateUpdated()
    {
        var (roomId, roomCode) = await SeedRoomAsync("TEST123");
        var gameId = Guid.NewGuid();

        var gameDto = new MultiplayerGameDto
        {
            GameId = gameId,
            RoomId = roomId,
            CurrentRound = 1,
            TotalRounds = 5,
            RoundLatitude = 54.0,
            RoundLongitude = 24.0,
            Players = new List<MultiplayerPlayerDto>()
        };

        _mockGameService.Setup(s => s.StartGameAsync(roomId))
            .ReturnsAsync(gameDto);

        await _hub.StartGame(roomCode);

        _mockGroupProxy.Verify(c => c.SendCoreAsync(
                "GameStarted",
                It.Is<object[]>(args => args.Length == 1 && (string)args[0] == gameId.ToString()),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockGroupProxy.Verify(c => c.SendCoreAsync(
                "RoundStarted",
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockGroupProxy.Verify(c => c.SendCoreAsync(
                "GameStateUpdated",
                It.Is<object[]>(args => args.Length == 1 && ReferenceEquals(args[0], gameDto)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetCurrentGame_ValidRoomId_AddsToGroup_And_SendsGameStateToCaller()
    {
        var roomId = Guid.NewGuid();
        var connectionId = "conn-123";
        _mockContext.SetupGet(c => c.ConnectionId).Returns(connectionId);

        var game = MakeGame(Guid.NewGuid(), roomId, currentRound: 1, totalRounds: 5);

        _mockGameService.Setup(s => s.GetCurrentGameForRoomAsync(roomId))
            .ReturnsAsync(game);

        await _hub.GetCurrentGame(roomId);

        _mockGroups.Verify(g =>
                g.AddToGroupAsync(connectionId, roomId.ToString(), It.IsAny<CancellationToken>()),
            Times.Once);

        _mockCallerProxy.Verify(c => c.SendCoreAsync(
                "GameStateUpdated",
                It.Is<object[]>(args => args.Length == 1 && ReferenceEquals(args[0], game)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task JoinGame_ValidRoomAndGame_SendsGameStateAndRoundStarted_ToCaller()
    {
        var (roomId, roomCode) = await SeedRoomAsync("TEST123");
        var gameId = Guid.NewGuid();
        var connectionId = "conn-123";
        _mockContext.SetupGet(c => c.ConnectionId).Returns(connectionId);

        var game = MakeGame(
            gameId: gameId,
            roomId: roomId,
            currentRound: 1,
            totalRounds: 5,
            roundLat: 54.0,
            roundLng: 24.0,
            players: new List<MultiplayerPlayer>());

        _mockGameService.Setup(s => s.GetCurrentGameForRoomAsync(roomId))
            .ReturnsAsync(game);

        await _hub.JoinGame(roomCode, gameId.ToString());

        _mockGroups.Verify(g =>
                g.AddToGroupAsync(connectionId, roomId.ToString(), It.IsAny<CancellationToken>()),
            Times.Once);

        _mockCallerProxy.Verify(c => c.SendCoreAsync(
                "GameStateUpdated",
                It.Is<object[]>(args => args.Length == 1 && args[0] is MultiplayerGameDto),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockCallerProxy.Verify(c => c.SendCoreAsync(
                "RoundStarted",
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SubmitGuess_ValidGuess_BroadcastsRoundResult_And_GameStateUpdated_ToGroup()
    {
        var playerId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var gameId = Guid.NewGuid();

        var guessResult = new RoundResultDto
        {
            PlayerId = playerId,
            DistanceMeters = 500,
            Score = 4500
        };

        var player = new MultiplayerPlayer
        {
            PlayerId = playerId,
            GameId = gameId,
            Score = 4500,
            Finished = true,
            Player = new Player { Id = playerId, DisplayName = "Player1" }
        };

        var game = MakeGame(
            gameId: gameId,
            roomId: roomId,
            currentRound: 1,
            totalRounds: 5,
            roundLat: 54.1,
            roundLng: 24.1,
            players: new List<MultiplayerPlayer> { player });

        var nextRoundDto = new MultiplayerGameDto
        {
            GameId = gameId,
            RoomId = roomId,
            CurrentRound = 2,
            TotalRounds = 5,
            RoundLatitude = 55.0,
            RoundLongitude = 25.0,
            Players = new List<MultiplayerPlayerDto>()
        };

        _mockGameService.Setup(s => s.NextRoundAsync(gameId))
            .ReturnsAsync(nextRoundDto);

        _mockGameService.Setup(s => s.SubmitGuessAsync(playerId, 54.0, 24.0))
            .ReturnsAsync(guessResult);

        _mockGameService.Setup(s => s.GetCurrentGameForPlayerAsync(playerId))
            .ReturnsAsync(player);

        _mockGameService.Setup(s => s.GetCurrentGameForRoomAsync(roomId))
            .ReturnsAsync(game);

        Console.WriteLine($"player.Game is null: {player.Game == null}");
        Console.WriteLine($"player.Game.RoomId: {player.Game?.RoomId}");

        await _hub.SubmitGuess(playerId, 54.0, 24.0);

        _mockGroupProxy.Verify(c => c.SendCoreAsync(
                "RoundResult",
                It.Is<object[]>(args => args.Length == 1 && ReferenceEquals(args[0], guessResult)),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockGroupProxy.Verify(c => c.SendCoreAsync(
                "GameStateUpdated",
                It.Is<object[]>(args => args.Length == 1 && args[0] is MultiplayerGameDto),
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task SubmitGuess_AllPlayersFinished_StartsNextRound_BroadcastsUpdates()
    {
        var playerId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var gameId = Guid.NewGuid();

        var guessResult = new RoundResultDto { PlayerId = playerId, Score = 4500 };

        var player = new MultiplayerPlayer
        {
            PlayerId = playerId,
            GameId = gameId,
            Finished = true,
            Player = new Player { DisplayName = "Player1" }
        };

        var game = MakeGame(
            gameId: gameId,
            roomId: roomId,
            currentRound: 1,
            totalRounds: 5,
            roundLat: 54.0,
            roundLng: 24.0,
            players: new List<MultiplayerPlayer> { player });

        var nextRoundDto = new MultiplayerGameDto
        {
            GameId = gameId,
            RoomId = roomId,
            CurrentRound = 2,
            TotalRounds = 5,
            RoundLatitude = 55.0,
            RoundLongitude = 25.0,
            Players = new List<MultiplayerPlayerDto>()
        };

        _mockGameService.Setup(s => s.SubmitGuessAsync(playerId, 54.0, 24.0))
            .ReturnsAsync(guessResult);

        _mockGameService.Setup(s => s.GetCurrentGameForPlayerAsync(playerId))
            .ReturnsAsync(player);

        _mockGameService.Setup(s => s.GetCurrentGameForRoomAsync(roomId))
            .ReturnsAsync(game);

        _mockGameService.Setup(s => s.NextRoundAsync(gameId))
            .ReturnsAsync(nextRoundDto);

        await _hub.SubmitGuess(playerId, 54.0, 24.0);

        _mockGameService.Verify(s => s.NextRoundAsync(gameId), Times.Once);


        _mockGroupProxy.Verify(c => c.SendCoreAsync(
                "GameStateUpdated",
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()),
            Times.AtLeast(2));

        _mockGroupProxy.Verify(c => c.SendCoreAsync(
                "RoundStarted",
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SubmitGuess_LastRoundCompleted_EndsGame_BroadcastsGameFinished()
    {
        var playerId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var gameId = Guid.NewGuid();

        var guessResult = new RoundResultDto { PlayerId = playerId, Score = 4500 };

        var player = new MultiplayerPlayer
        {
            PlayerId = playerId,
            GameId = gameId,
            Finished = true,
            Player = new Player { DisplayName = "Player1" }
        };

        var game = MakeGame(
            gameId: gameId,
            roomId: roomId,
            currentRound: 5,
            totalRounds: 5,
            players: new List<MultiplayerPlayer> { player });

        var finishedGameDto = new MultiplayerGameDto
        {
            GameId = gameId,
            RoomId = roomId,
            CurrentRound = 5,
            TotalRounds = 5,
            Players = new List<MultiplayerPlayerDto>()
        };

        _mockGameService.Setup(s => s.SubmitGuessAsync(playerId, 54.0, 24.0))
            .ReturnsAsync(guessResult);

        _mockGameService.Setup(s => s.GetCurrentGameForPlayerAsync(playerId))
            .ReturnsAsync(player);

        _mockGameService.Setup(s => s.GetCurrentGameForRoomAsync(roomId))
            .ReturnsAsync(game);

        _mockGameService.Setup(s => s.EndGameAsync(gameId))
            .ReturnsAsync(finishedGameDto);

        await _hub.SubmitGuess(playerId, 54.0, 24.0);

        _mockGameService.Verify(s => s.EndGameAsync(gameId), Times.Once);

        _mockGroupProxy.Verify(c => c.SendCoreAsync(
                "GameFinished",
                It.Is<object[]>(args => args.Length == 1 && ReferenceEquals(args[0], finishedGameDto)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
