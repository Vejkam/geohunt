using Microsoft.AspNetCore.Mvc;
using Moq;
using psi25_project.Controllers;
using psi25_project.Models.Dtos;
using psi25_project.Services.Interfaces;

namespace Geohunt.Tests.Controllers
{
    public class RoomsControllerTests
    {
        private readonly Mock<IRoomService> _mockRoomService;
        private readonly RoomsController _controller;

        public RoomsControllerTests()
        {
            _mockRoomService = new Mock<IRoomService>();
            _controller = new RoomsController(_mockRoomService.Object);
        }

        [Fact]
        public async Task CreateRoom_ReturnsOkWithRoomDto()
        {
            // Arrange
            var dto = new RoomCreateDto { TotalRounds = 3 };
            var roomDto = new RoomDto { Id = Guid.NewGuid(), RoomCode = "ABCDE", TotalRounds = 3, CurrentRounds = 1 };

            _mockRoomService.Setup(s => s.CreateRoomAsync(dto)).ReturnsAsync(roomDto);

            // Act
            var result = await _controller.CreateRoom(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedRoom = Assert.IsType<RoomDto>(okResult.Value);
            Assert.Equal(roomDto.Id, returnedRoom.Id);
            Assert.Equal(roomDto.RoomCode, returnedRoom.RoomCode);
        }

        [Fact]
        public async Task JoinRoom_PlayerFound_ReturnsOkWithPlayerDto()
        {
            // Arrange
            var dto = new JoinRoomDto { RoomCode = "ABCDE", UserId = Guid.NewGuid(), DisplayName = "TestPlayer" };
            var playerDto = new PlayerDto { Id = Guid.NewGuid(), UserId = dto.UserId, DisplayName = "TestPlayer", IsReady = false };

            _mockRoomService.Setup(s => s.JoinRoomAsync(dto)).ReturnsAsync(playerDto);

            // Act
            var result = await _controller.JoinRoom(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPlayer = Assert.IsType<PlayerDto>(okResult.Value);
            Assert.Equal(playerDto.Id, returnedPlayer.Id);
            Assert.Equal(playerDto.DisplayName, returnedPlayer.DisplayName);
        }

        [Fact]
        public async Task JoinRoom_RoomNotFound_ReturnsNotFound()
        {
            // Arrange
            var dto = new JoinRoomDto { RoomCode = "WRONG", UserId = Guid.NewGuid(), DisplayName = "Player" };
            _mockRoomService.Setup(s => s.JoinRoomAsync(dto)).ReturnsAsync((PlayerDto?)null);

            // Act
            var result = await _controller.JoinRoom(dto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            dynamic value = notFoundResult.Value!;
            Assert.Equal("Room not found", value.message);
        }

        [Fact]
        public async Task GetPlayers_ReturnsOkWithListOfPlayers()
        {
            // Arrange
            string roomCode = "ABCDE";
            var players = new List<PlayerDto>
            {
                new() { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), DisplayName = "Alice", IsReady = false },
                new() { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), DisplayName = "Bob", IsReady = true }
            };

            _mockRoomService.Setup(s => s.GetPlayersInRoomAsync(roomCode)).ReturnsAsync(players);

            // Act
            var result = await _controller.GetPlayers(roomCode);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPlayers = Assert.IsType<List<PlayerDto>>(okResult.Value);
            Assert.Equal(2, returnedPlayers.Count);
            Assert.Contains(returnedPlayers, p => p.DisplayName == "Alice");
            Assert.Contains(returnedPlayers, p => p.DisplayName == "Bob");
        }

        [Fact]
        public async Task GetPlayers_EmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            string roomCode = "EMPTY";
            _mockRoomService.Setup(s => s.GetPlayersInRoomAsync(roomCode)).ReturnsAsync(new List<PlayerDto>());

            // Act
            var result = await _controller.GetPlayers(roomCode);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedPlayers = Assert.IsType<List<PlayerDto>>(okResult.Value);
            Assert.Empty(returnedPlayers);
        }
    }
}
