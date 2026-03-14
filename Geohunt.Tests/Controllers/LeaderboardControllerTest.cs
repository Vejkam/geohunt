using Microsoft.AspNetCore.Mvc;
using Moq;
using psi25_project.Controllers;
using psi25_project.Models.Dtos;
using psi25_project.Services.Interfaces;

namespace Geohunt.Tests.Controllers
{
    public class LeaderboardControllerTests
    {
        private readonly Mock<ILeaderboardService> _mockService;
        private readonly LeaderboardController _controller;

        public LeaderboardControllerTests()
        {
            _mockService = new Mock<ILeaderboardService>();
            _controller = new LeaderboardController(_mockService.Object);
        }

        [Fact]
        public async Task GetLeaderboard_ReturnsOk_WithLeaderboardData()
        {
            // Arrange
            var leaderboardData = new List<LeaderboardEntry>
            {
                new LeaderboardEntry { Id = 1, Username = "Player1", TotalScore = 100, DistanceKm = 5 },
                new LeaderboardEntry { Id = 2, Username = "Player2", TotalScore = 90, DistanceKm = 10 }
            };

            _mockService.Setup(s => s.GetTopLeaderboardAsync(It.IsAny<int>()))
                        .ReturnsAsync(leaderboardData);

            // Act
            var result = await _controller.GetLeaderboard();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<LeaderboardEntry>>(okResult.Value);

            Assert.Equal(2, value.Count);
            Assert.Equal("Player1", value[0].Username);
        }

        [Fact]
        public async Task GetTopPlayers_ReturnsOk_WithTopPlayers()
        {
            // Arrange
            int top = 3;
            var topPlayers = new List<LeaderboardEntry>
            {
                new LeaderboardEntry { Id = 1, Username = "Player1", TotalScore = 100 },
                new LeaderboardEntry { Id = 2, Username = "Player2", TotalScore = 90 },
                new LeaderboardEntry { Id = 3, Username = "Player3", TotalScore = 80 }
            };

            _mockService.Setup(s => s.GetTopPlayersAsync(It.IsAny<int>()))
                        .ReturnsAsync(topPlayers);

            // Act
            var result = await _controller.GetTopPlayers(top);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<List<LeaderboardEntry>>(okResult.Value);

            Assert.Equal(top, value.Count);
            Assert.Equal("Player1", value[0].Username);

            _mockService.Verify(s => s.GetTopPlayersAsync(top), Times.Once);
        }

        [Fact]
        public async Task GetTopPlayers_UsesDefaultTop()
        {
            // Arrange
            var expectedTop = 20;
            var players = new List<LeaderboardEntry>();

            _mockService.Setup(s => s.GetTopPlayersAsync(It.IsAny<int>()))
                        .ReturnsAsync(players);

            // Act
            var result = await _controller.GetTopPlayers();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<List<LeaderboardEntry>>(okResult.Value);

            _mockService.Verify(s => s.GetTopPlayersAsync(expectedTop), Times.Once);
        }
    }
}
