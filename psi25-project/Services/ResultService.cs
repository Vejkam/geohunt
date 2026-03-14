using psi25_project.Models.Dtos;
using psi25_project.Services.Interfaces;
using psi25_project.Utils;

namespace psi25_project.Services
{
    public class ResultService : IResultService
    {
        public (double distance, int score) CalculateResult(DistanceDto dto)
        {
            var coords1 = dto.initialCoords;
            var coords2 = dto.guessedCoords;

            var distance = DistanceCalculator.CalculateHaversineDistance(coords1, coords2, precision: 2);

            var score = ScoreCalculator.CalculateGeoScore(distance);

            return (distance, score);
        }
    }
}
