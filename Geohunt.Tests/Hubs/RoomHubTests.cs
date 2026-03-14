using Moq;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using psi25_project.Hubs;
using psi25_project.Services.Interfaces;
using psi25_project.Data;
using psi25_project.Models;

namespace Geohunt.Tests.Hubs;

public class RoomHubTests
{
    private readonly Mock<IRoomOnlineService> _mockRoomOnlineService;
    private readonly Mock<IHubCallerClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly Mock<IGroupManager> _mockGroups;
    private readonly Mock<HubCallerContext> _mockContext;
    private readonly GeoHuntContext _context;
    private readonly RoomHub _hub;

    public RoomHubTests()
    {
        _mockRoomOnlineService = new Mock<IRoomOnlineService>();
        _mockClients = new Mock<IHubCallerClients>();
        _mockClientProxy = new Mock<IClientProxy>();
        _mockGroups = new Mock<IGroupManager>();
        _mockContext = new Mock<HubCallerContext>();

        var options = new DbContextOptionsBuilder<GeoHuntContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new GeoHuntContext(options);

        _hub = new RoomHub(_mockRoomOnlineService.Object, _context)
        {
            Clients = _mockClients.Object,
            Groups = _mockGroups.Object,
            Context = _mockContext.Object
        };

        _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
    }

    [Fact]
    public async Task JoinRoom_ValidRoom_AddsPlayerAndNotifies()
    {
        // Arrange
        var roomCode = "TEST123";
        var roomId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var displayName = "TestPlayer";
        var connectionId = "conn-123";

        var room = new Room
        {
            Id = roomId,
            RoomCode = roomCode,
            Players = new List<Player>
            {
                new Player { Id = playerId, DisplayName = displayName, UserId = playerId }
            },
            RowVersion = new byte[] {1}
        };

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        _mockContext.Setup(c => c.ConnectionId).Returns(connectionId);
        _mockRoomOnlineService.Setup(s => s.GetOnlinePlayers(roomId.ToString()))
            .Returns(new List<PlayerOnline>());

        // Act
        await _hub.JoinRoom(roomCode, playerId, displayName);

        // Assert
        _mockGroups.Verify(
            g => g.AddToGroupAsync(connectionId, roomId.ToString(), default),
            Times.Once);

        _mockRoomOnlineService.Verify(
            s => s.AddOnlinePlayer(
                roomId.ToString(),
                It.Is<PlayerOnline>(p =>
                    p.PlayerId == playerId &&
                    p.DisplayName == displayName &&
                    p.ConnectionId == connectionId &&
                    p.IsReady == false)),
            Times.Once);

        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                "PlayerListUpdated",
                It.IsAny<object[]>(),
                default),
            Times.Once);
    }

    [Fact]
    public async Task JoinRoom_InvalidRoom_ThrowsHubException()
    {
        // Arrange
        var invalidRoomCode = "INVALID";
        var playerId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HubException>(
            () => _hub.JoinRoom(invalidRoomCode, playerId, "Player"));
        
        Assert.Equal("Room not found", exception.Message);
    }

    [Fact]
    public async Task JoinRoom_PlayerAlreadyInRoom_RemovesOldConnection()
    {
        // Arrange
        var roomCode = "TEST123";
        var roomId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var displayName = "TestPlayer";
        var oldConnectionId = "old-conn";
        var newConnectionId = "new-conn";

        var room = new Room
        {
            Id = roomId,
            RoomCode = roomCode,
            Players = new List<Player>
            {
                new Player { Id = playerId, DisplayName = displayName, UserId = playerId }
            },
            RowVersion = new byte[] {1}
        };

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        var existingPlayer = new PlayerOnline
        {
            PlayerId = playerId,
            DisplayName = displayName,
            ConnectionId = oldConnectionId
        };

        _mockContext.Setup(c => c.ConnectionId).Returns(newConnectionId);
        _mockRoomOnlineService.Setup(s => s.GetOnlinePlayers(roomId.ToString()))
            .Returns(new List<PlayerOnline> { existingPlayer });

        // Act
        await _hub.JoinRoom(roomCode, playerId, displayName);

        // Assert
        _mockRoomOnlineService.Verify(
            s => s.RemoveOnlinePlayer(roomId.ToString(), oldConnectionId),
            Times.Once);
    }

    [Fact]
    public async Task LeaveRoom_ValidRoom_RemovesPlayerAndNotifies()
    {
        // Arrange
        var roomCode = "TEST123";
        var roomId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var connectionId = "conn-123";

        var room = new Room
        {
            Id = roomId,
            RoomCode = roomCode,
            Players = new List<Player>
            {
                new Player { Id = playerId, DisplayName = "Player1", UserId = playerId }
            },
            RowVersion = new byte[] {1}
        };

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        _mockContext.Setup(c => c.ConnectionId).Returns(connectionId);
        _mockRoomOnlineService.Setup(s => s.RemoveOnlinePlayer(roomId.ToString(), connectionId))
            .Returns(new PlayerOnline { PlayerId = playerId, ConnectionId = connectionId });

        // Act
        await _hub.LeaveRoom(roomCode);

        // Assert
        _mockRoomOnlineService.Verify(
            s => s.RemoveOnlinePlayer(roomId.ToString(), connectionId),
            Times.Once);

        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                "PlayerLeft",
                It.Is<object[]>(o => (Guid)o[0] == playerId),
                default),
            Times.Once);

        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                "PlayerListUpdated",
                It.IsAny<object[]>(),
                default),
            Times.Once);

        _mockGroups.Verify(
            g => g.RemoveFromGroupAsync(connectionId, roomId.ToString(), default),
            Times.Once);
    }

    [Fact]
    public async Task UpdateReadyState_ValidRoom_UpdatesStateAndNotifies()
    {
        // Arrange
        var roomCode = "TEST123";
        var roomId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var isReady = true;

        var room = new Room
        {
            Id = roomId,
            RoomCode = roomCode,
            Players = new List<Player>
            {
                new Player { Id = playerId, DisplayName = "Player1", UserId = playerId, IsReady = false }
            },
            RowVersion = new byte[] {1}
        };

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        // Act
        await _hub.UpdateReadyState(roomCode, playerId, isReady);

        // Assert
        _mockRoomOnlineService.Verify(
            s => s.UpdatePlayerState(roomId.ToString(), playerId, isReady),
            Times.Once);

        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                "PlayerListUpdated",
                It.IsAny<object[]>(),
                default),
            Times.Once);
    }

    [Fact]
    public async Task OnDisconnectedAsync_PlayerInRooms_RemovesFromAllRooms()
    {
        // Arrange
        var connectionId = "conn-123";
        var roomId1 = Guid.NewGuid().ToString();
        var roomId2 = Guid.NewGuid().ToString();
        var playerId = Guid.NewGuid();

        // Create ONE player instance
        var player = new Player 
        { 
            Id = playerId, 
            DisplayName = "Player1", 
            UserId = playerId 
        };

        var room1 = new Room
        {
            Id = Guid.Parse(roomId1),
            RoomCode = "ROOM1",
            Players = new List<Player> { player },
            RowVersion = new byte[] {1}
        };

        var room2 = new Room
        {
            Id = Guid.Parse(roomId2),
            RoomCode = "ROOM2",
            Players = new List<Player> { player },  
            RowVersion = new byte[] {1}
        };

        _context.Rooms.AddRange(room1, room2);
        await _context.SaveChangesAsync();

        _mockContext.Setup(c => c.ConnectionId).Returns(connectionId);
        _mockRoomOnlineService.Setup(s => s.GetRoomsForConnection(connectionId))
            .Returns(new List<string> { roomId1, roomId2 });
        _mockRoomOnlineService.Setup(s => s.RemoveOnlinePlayer(It.IsAny<string>(), connectionId))
            .Returns(new PlayerOnline { PlayerId = playerId, ConnectionId = connectionId });

        // Act
        await _hub.OnDisconnectedAsync(null);

        // Assert
        _mockRoomOnlineService.Verify(
            s => s.RemoveOnlinePlayer(roomId1, connectionId),
            Times.Once);
        _mockRoomOnlineService.Verify(
            s => s.RemoveOnlinePlayer(roomId2, connectionId),
            Times.Once);

        _mockClientProxy.Verify(
            c => c.SendCoreAsync(
                "PlayerLeft",
                It.Is<object[]>(o => (Guid)o[0] == playerId),
                default),
            Times.Exactly(2));
    }
}