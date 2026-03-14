namespace psi25_project.Models.Dtos
{
    public class CreateGameDto
    {
        public Guid UserId { get; set; }
        public int TotalRounds { get; set; } = 3;
    }
}
