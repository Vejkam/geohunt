namespace psi25_project.Models.Dtos
{
    public class JoinRoomDto
    {
        public string? RoomCode { get; set; }
        public Guid UserId { get; set; }
        public string? DisplayName { get; set; }
    }
}
