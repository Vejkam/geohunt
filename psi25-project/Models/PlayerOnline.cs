namespace psi25_project.Models
{
    public class PlayerOnline
    {
        public Guid PlayerId { get; set; }
        public string DisplayName { get; set; } = "";
        public string ConnectionId { get; set; } = "";
        public bool IsReady { get; set; } = false;
    }
}
