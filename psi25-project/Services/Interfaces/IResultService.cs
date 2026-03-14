using psi25_project.Models.Dtos;

namespace psi25_project.Services.Interfaces
{
    public interface IResultService
    {
        (double distance, int score) CalculateResult(DistanceDto dto);
    }
}
