using System.Text.Json.Serialization;

namespace psi25_project.Models
{
    public class Player
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        [JsonIgnore]
        public ApplicationUser? User { get; set; }
        public Guid? RoomId { get; set; }
        [JsonIgnore]
        public Room? Room { get; set; }
        public int Score { get; set; }
        public bool IsReady { get; set; }
        public string DisplayName { get; set; } = "";
    }
}