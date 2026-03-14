using Moq;
using psi25_project.Models.Dtos;
using psi25_project.Repositories.Interfaces;
using psi25_project.Services;

namespace Geohunt.Tests.Services;

public class LeaderboardServiceTest
{
    private readonly Mock<ILeaderboardRepository> _mockRepo;
    private readonly LeaderboardService _service;

    public LeaderboardServiceTest()
    {
        _mockRepo = new Mock<ILeaderboardRepository>();
        _service = new LeaderboardService(_mockRepo.Object);
    }

    [Fact]
    public async Task GetTopLeaderboardAsync_ReturnsExpectedResults()
    {
        var fakeLeaderboard = new List<LeaderboardEntry>
        {
            new LeaderboardEntry { Id = 1, Username = "Joe", TotalScore = 100 },
            new LeaderboardEntry { Id = 2, Username = "Doe", TotalScore = 80 }
        };
        
        _mockRepo.Setup(r => r.GetTopGuessesAsync(2))
                 .ReturnsAsync(fakeLeaderboard);

        var result = await _service.GetTopLeaderboardAsync(2);

        Assert.Equal(2, result.Count);
        Assert.Equal("Joe", result[0].Username);
        Assert.Equal(100, result[0].TotalScore);

         _mockRepo.Verify(r => r.GetTopGuessesAsync(2), Times.Once);
    }

    [Fact]
    public async Task GetTopPlayersAsync_ReturnExpectedResults()
    {
        var fakePlayers = new List<LeaderboardEntry>
        {
            new LeaderboardEntry { Id = 1, Username = "Joe", TotalScore = 150 },
        };

        _mockRepo.Setup(r => r.GetTopPlayersAsync(1))
                 .ReturnsAsync(fakePlayers);

        var result = await _service.GetTopPlayersAsync(1);

        Assert.Single(result);
        Assert.Equal("Joe", result[0].Username);

        _mockRepo.Verify(r => r.GetTopPlayersAsync(1), Times.Once);
    }
}