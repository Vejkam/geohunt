using psi25_project.Models;
using psi25_project.Services;

namespace Geohunt.Tests.Services;

public class RoomOnlineServiceTests
{
    private readonly RoomOnlineService _service;

    public RoomOnlineServiceTests()
    {
        _service = new RoomOnlineService();
    }

    [Fact]
    public void AddOnlinePlayer_NewRoom_AddsPlayer()
    {
        // Arrange
        var roomId = "room1";
        var player = new PlayerOnline
        {
            PlayerId = Guid.NewGuid(),
            DisplayName = "Alice",
            ConnectionId = "conn1",
            IsReady = false
        };

        // Act
        _service.AddOnlinePlayer(roomId, player);
        var players = _service.GetOnlinePlayers(roomId);

        // Assert
        Assert.Single(players);
        Assert.Equal(player.ConnectionId, players[0].ConnectionId);
    }

    [Fact]
    public void AddOnlinePlayer_SameConnection_DoesNotDuplicate()
    {
        // Arrange
        var roomId = "room1";
        var player = new PlayerOnline
        {
            PlayerId = Guid.NewGuid(),
            DisplayName = "Alice",
            ConnectionId = "conn1",
            IsReady = false
        };

        // Act
        _service.AddOnlinePlayer(roomId, player);
        _service.AddOnlinePlayer(roomId, player);
        var players = _service.GetOnlinePlayers(roomId);

        // Assert
        Assert.Single(players);
    }

    [Fact]
    public void RemoveOnlinePlayer_PlayerExists_RemovesAndReturnsPlayer()
    {
        // Arrange
        var roomId = "room1";
        var player = new PlayerOnline
        {
            PlayerId = Guid.NewGuid(),
            DisplayName = "Bob",
            ConnectionId = "conn2",
            IsReady = false
        };

        _service.AddOnlinePlayer(roomId, player);

        // Act
        var removed = _service.RemoveOnlinePlayer(roomId, player.ConnectionId);
        var players = _service.GetOnlinePlayers(roomId);

        // Assert
        Assert.NotNull(removed);
        Assert.Equal(player.ConnectionId, removed!.ConnectionId);
        Assert.Empty(players);
    }

    [Fact]
    public void RemoveOnlinePlayer_LastPlayer_RemovesRoom()
    {
        // Arrange
        var roomId = "room1";
        var player = new PlayerOnline
        {
            PlayerId = Guid.NewGuid(),
            DisplayName = "Charlie",
            ConnectionId = "conn3",
            IsReady = false
        };

        _service.AddOnlinePlayer(roomId, player);

        // Act
        _service.RemoveOnlinePlayer(roomId, player.ConnectionId);
        var rooms = _service.GetAllRooms();

        // Assert
        Assert.DoesNotContain(roomId, rooms);
    }

    [Fact]
    public void UpdatePlayerState_PlayerExists_UpdatesReadyState()
    {
        // Arrange
        var roomId = "room1";
        var playerId = Guid.NewGuid();
        var player = new PlayerOnline
        {
            PlayerId = playerId,
            DisplayName = "Dana",
            ConnectionId = "conn4",
            IsReady = false
        };

        _service.AddOnlinePlayer(roomId, player);

        // Act
        _service.UpdatePlayerState(roomId, playerId, true);
        var players = _service.GetOnlinePlayers(roomId);

        // Assert
        Assert.True(players.Single().IsReady);
    }

    [Fact]
    public void GetOnlinePlayers_ReturnsCopy_NotReference()
    {
        // Arrange
        var roomId = "room1";
        var player = new PlayerOnline
        {
            PlayerId = Guid.NewGuid(),
            DisplayName = "Eve",
            ConnectionId = "conn5",
            IsReady = false
        };

        _service.AddOnlinePlayer(roomId, player);

        // Act
        var players = _service.GetOnlinePlayers(roomId);
        players[0].IsReady = true;

        var freshPlayers = _service.GetOnlinePlayers(roomId);

        // Assert
        Assert.False(freshPlayers[0].IsReady);
    }

    [Fact]
    public void GetRoomsForConnection_ReturnsOnlyRoomsWithThatConnection()
    {
        // Arrange
        var connectionId = "connX";

        _service.AddOnlinePlayer("room1", new PlayerOnline
        {
            PlayerId = Guid.NewGuid(),
            DisplayName = "A",
            ConnectionId = connectionId
        });

        _service.AddOnlinePlayer("room2", new PlayerOnline
        {
            PlayerId = Guid.NewGuid(),
            DisplayName = "B",
            ConnectionId = "other"
        });

        _service.AddOnlinePlayer("room3", new PlayerOnline
        {
            PlayerId = Guid.NewGuid(),
            DisplayName = "C",
            ConnectionId = connectionId
        });

        // Act
        var rooms = _service.GetRoomsForConnection(connectionId);

        // Assert
        Assert.Equal(2, rooms.Count);
        Assert.Contains("room1", rooms);
        Assert.Contains("room3", rooms);
        Assert.DoesNotContain("room2", rooms);
    }

    [Fact]
    public void GetOnlinePlayers_RoomDoesNotExist_ReturnsEmptyList()
    {
        // Act
        var players = _service.GetOnlinePlayers("unknown");

        // Assert
        Assert.NotNull(players);
        Assert.Empty(players);
    }
}
