namespace psi25_project.Models.Dtos
{
    public class GameResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int TotalScore { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public int CurrentRound { get; set; }
        public int TotalRounds { get; set; }
    }
}