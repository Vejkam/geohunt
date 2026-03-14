using Microsoft.AspNetCore.Mvc;
using psi25_project.Services.Interfaces;


namespace psi25_project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;

        public LeaderboardController(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        [HttpGet("top-scores")]
        public async Task<ActionResult> GetLeaderboard()
        {
            var leaderboard = await _leaderboardService.GetTopLeaderboardAsync();
            return Ok(leaderboard);
        }
        
        [HttpGet("top-players")]
        public async Task<IActionResult> GetTopPlayers([FromQuery] int top = 20)
        {
            var result = await _leaderboardService.GetTopPlayersAsync(top);
            return Ok(result);
        }
    }
}
