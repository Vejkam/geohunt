namespace psi25_project.Models.Dtos
{
    public class RoomDto
    {
        public Guid Id { get; set; }
        public string? RoomCode { get; set; }
        public int TotalRounds { get; set; }
        public int CurrentRounds { get; set; }
    }
}
