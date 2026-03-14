using psi25_project.Utils;
using Xunit;

namespace Geohunt.Tests.Utils
{
    public class ScoreCalculatorTests
    {
        [Theory]
        [InlineData(0, 5000)]       // Distance 0 should give max score
        [InlineData(1000, 4094)]    // Some arbitrary distance
        [InlineData(5000, 1839)]    // Distance = BIGGEST_DISTANCE
        [InlineData(10000, 677)]    // Larger distance
        [InlineData(20000, 92)]     // Very large distance
        public void CalculateGeoScore_ReturnsExpectedScore(double distance, int expectedScore)
        {
            // Act
            int score = ScoreCalculator.CalculateGeoScore(distance);

            // Assert
            Assert.Equal(expectedScore, score);
        }

        [Fact]
        public void CalculateGeoScore_NeverExceedsMaxScore()
        {
            int score = ScoreCalculator.CalculateGeoScore(-1000); // negative distance should be clamped
            Assert.Equal(5000, score);
        }

        [Fact]
        public void CalculateGeoScore_NeverBelowZero()
        {
            int score = ScoreCalculator.CalculateGeoScore(1e6); // huge distance
            Assert.True(score >= 0);
        }
    }
}
