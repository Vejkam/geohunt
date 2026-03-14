using Moq;
using psi25_project.Models;
using psi25_project.Models.Dtos;
using psi25_project.Repositories.Interfaces;
using psi25_project.Services;

namespace Geohunt.Tests.Services;

public class RoomServiceTests
{
    private readonly Mock<IRoomRepository> _mockRoomRepository;
    private readonly Mock<IPlayerRepository> _mockPlayerRepository;
    private readonly RoomService _service;

    public RoomServiceTests()
    {
        _mockRoomRepository = new Mock<IRoomRepository>();
        _mockPlayerRepository = new Mock<IPlayerRepository>();
        _service = new RoomService(_mockRoomRepository.Object, _mockPlayerRepository.Object);
    }

    [Fact]
    public async Task CreateRoomAsync_CreatesRoom_WithExpectedDefaults()
    {
        // Arrange
        var dto = new RoomCreateDto
        {
            TotalRounds = 3
        };

        Room? capturedRoom = null;

        _mockRoomRepository
            .Setup(r => r.CreateRoomAsync(It.IsAny<Room>()))
            .Callback<Room>(r => capturedRoom = r)
            .ReturnsAsync((Room r) => r);

        // Act
        var result = await _service.CreateRoomAsync(dto);

        // Assert
        Assert.NotNull(capturedRoom);
        Assert.Equal(dto.TotalRounds, capturedRoom!.TotalRounds);
        Assert.Equal(1, capturedRoom.CurrentRounds);
        Assert.False(string.IsNullOrWhiteSpace(capturedRoom.RoomCode));
        Assert.Equal(5, capturedRoom.RoomCode.Length);

        // Assert
        Assert.Equal(capturedRoom.Id, result.Id);
        Assert.Equal(capturedRoom.RoomCode, result.RoomCode);
        Assert.Equal(capturedRoom.TotalRounds, result.TotalRounds);
        Assert.Equal(capturedRoom.CurrentRounds, result.CurrentRounds);

        _mockRoomRepository.Verify(r => r.CreateRoomAsync(It.IsAny<Room>()), Times.Once);
    }

    [Fact]
    public async Task JoinRoomAsync_RoomNotFound_ReturnsNull()
    {
        // Arrange
        var dto = new JoinRoomDto
        {
            RoomCode = "ABCDE",
            UserId = Guid.NewGuid(),
            DisplayName = "Player"
        };

        _mockRoomRepository
            .Setup(r => r.GetRoomByCodeAsync(dto.RoomCode))
            .ReturnsAsync((Room?)null);

        // Act
        var result = await _service.JoinRoomAsync(dto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task JoinRoomAsync_PlayerAlreadyExists_ReturnsExistingPlayer()
    {
        // Arrange
        var room = new Room { Id = Guid.NewGuid() };
        var player = new Player
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RoomId = room.Id,
            DisplayName = "Existing Player",
            IsReady = false
        };

        var dto = new JoinRoomDto
        {
            RoomCode = "ABCDE",
            UserId = player.UserId,
            DisplayName = "Existing Player"
        };

        _mockRoomRepository
            .Setup(r => r.GetRoomByCodeAsync(dto.RoomCode))
            .ReturnsAsync(room);

        _mockPlayerRepository
            .Setup(p => p.GetPlayerByUserAndRoomAsync(player.UserId, room.Id))
            .ReturnsAsync(player);

        // Act
        var result = await _service.JoinRoomAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(player.Id, result!.Id);
        Assert.Equal(player.DisplayName, result.DisplayName);
        Assert.False(result.IsReady);

        _mockPlayerRepository.Verify(
            p => p.AddPlayerAsync(It.IsAny<Player>()),
            Times.Never);
    }

    [Fact]
    public async Task JoinRoomAsync_NewPlayer_AddsPlayerAndReturnsDto()
    {
        // Arrange
        var room = new Room { Id = Guid.NewGuid() };

        var dto = new JoinRoomDto
        {
            RoomCode = "ABCDE",
            UserId = Guid.NewGuid(),
            DisplayName = "New Player"
        };

        Player? capturedPlayer = null;

        _mockRoomRepository
            .Setup(r => r.GetRoomByCodeAsync(dto.RoomCode))
            .ReturnsAsync(room);

        _mockPlayerRepository
            .Setup(p => p.GetPlayerByUserAndRoomAsync(dto.UserId, room.Id))
            .ReturnsAsync((Player?)null);

        _mockPlayerRepository
            .Setup(p => p.AddPlayerAsync(It.IsAny<Player>()))
            .Callback<Player>(p => capturedPlayer = p)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.JoinRoomAsync(dto);

        // Assert
        Assert.NotNull(capturedPlayer);
        Assert.Equal(dto.UserId, capturedPlayer!.UserId);
        Assert.Equal(dto.DisplayName, capturedPlayer.DisplayName);
        Assert.Equal(room.Id, capturedPlayer.RoomId);
        Assert.False(capturedPlayer.IsReady);
        Assert.Equal(0, capturedPlayer.Score);

        // Assert
        Assert.Equal(capturedPlayer.Id, result!.Id);
        Assert.Equal(capturedPlayer.DisplayName, result.DisplayName);

        _mockPlayerRepository.Verify(p => p.AddPlayerAsync(It.IsAny<Player>()), Times.Once);
    }

    [Fact]
    public async Task GetPlayersInRoomAsync_RoomNotFound_ReturnsEmptyList()
    {
        // Arrange
        _mockRoomRepository
            .Setup(r => r.GetRoomWithPlayersAsync("ABCDE"))
            .ReturnsAsync((Room?)null);

        // Act
        var result = await _service.GetPlayersInRoomAsync("ABCDE");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task SetReadyAsync_PlayerExists_SetsReadyToTrue()
    {
        // Arrange
        var player = new Player
        {
            Id = Guid.NewGuid(),
            IsReady = false
        };

        _mockPlayerRepository
            .Setup(p => p.GetPlayerByIdAsync(player.Id))
            .ReturnsAsync(player);

        _mockPlayerRepository
            .Setup(p => p.UpdatePlayerAsync(player))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.SetReadyAsync(player.Id);

        // Assert
        Assert.True(player.IsReady);
        Assert.True(result!.IsReady);

        _mockPlayerRepository.Verify(p => p.UpdatePlayerAsync(player), Times.Once);
    }

    [Fact]
    public async Task ToggleReadyAsync_TogglesReadyState()
    {
        // Arrange
        var player = new Player
        {
            Id = Guid.NewGuid(),
            IsReady = false
        };

        _mockPlayerRepository
            .Setup(p => p.GetPlayerByIdAsync(player.Id))
            .ReturnsAsync(player);

        // Act
        var result = await _service.ToggleReadyAsync(player.Id);

        // Assert
        Assert.True(player.IsReady);
        Assert.True(result!.IsReady);
    }

    [Fact]
    public async Task LeaveRoomAsync_LastPlayer_RemovesPlayerAndDeletesRoom()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var player = new Player
        {
            Id = Guid.NewGuid(),
            RoomId = roomId
        };

        _mockPlayerRepository
            .Setup(p => p.GetPlayerByIdAsync(player.Id))
            .ReturnsAsync(player);

        _mockPlayerRepository
            .Setup(p => p.GetPlayersByRoomIdAsync(roomId))
            .ReturnsAsync(new List<Player>());

        // Act
        var result = await _service.LeaveRoomAsync(player.Id);

        // Assert
        Assert.True(result);

        _mockPlayerRepository.Verify(p => p.RemovePlayerAsync(player.Id), Times.Once);
        _mockRoomRepository.Verify(r => r.DeleteRoomAsync(roomId), Times.Once);
    }
}
