using psi25_project.Models.Dtos;
using psi25_project.Services;
using psi25_project.Utils;

namespace Geohunt.Tests.Services;
public class ResultServiceTests
{
    [Fact]
    public void CalculateResult_ReturnsCorrectDistanceAndScore()
    {
        var service = new ResultService();

        var dto = new DistanceDto
        {
            initialCoords = new() { Lat = 52.5200, Lng = 13.4050 },
            guessedCoords = new() { Lat = 48.8566, Lng = 2.3522 }
        };

        double expectedDistance = DistanceCalculator.CalculateHaversineDistance(
            dto.initialCoords, 
            dto.guessedCoords, 
            precision: 2
        );

        int expectedScore = ScoreCalculator.CalculateGeoScore(expectedDistance);

        var (distance, score) = service.CalculateResult(dto);

        Assert.Equal(expectedDistance, distance);
        Assert.Equal(expectedScore, score);
    }

    [Fact]
    public void CalculateResult_ZeroDistance_ReturnsFullScore()
    {
        var service = new ResultService();

        var dto = new DistanceDto
        {
            initialCoords = new() { Lat = 50, Lng = 10 },
            guessedCoords = new() { Lat = 50, Lng = 10 }
        };

        var (distance, score) = service.CalculateResult(dto);

        Assert.Equal(0, distance);
        Assert.True(score > 0);
    }
}
