using Microsoft.AspNetCore.Identity;

namespace psi25_project.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public DateTime CreatedAt { get; set; }
        public List<Game> Games { get; set; } = new List<Game>();
        public DateTime? LastRoomJoinedTime { get; set; }
    }
}
