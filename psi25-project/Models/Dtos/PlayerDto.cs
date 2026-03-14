namespace psi25_project.Models.Dtos
{
    public class PlayerDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string? DisplayName { get; set; }
        public bool IsReady { get; set; }
    }
}
