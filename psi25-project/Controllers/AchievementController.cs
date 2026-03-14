using Microsoft.AspNetCore.Mvc;
using psi25_project.Models;
using psi25_project.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class AchievementController : ControllerBase
{
    private readonly IAchievementService _achievementService;
    public AchievementController(IAchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    [HttpGet("available-achievements")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AchievementDto>>> GetAllAvailableAchievements()
    {
        try
        {
            var achievements = await _achievementService.GetActiveAchievementsAsync();

            if (achievements.Count == 0)
                return NoContent();

            return Ok(achievements);
        }
        catch (Exception ex)
        {
            return Problem($"Failed to get available achievements: {ex.Message}");
        }
    }

    [HttpGet("achievements/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AchievementDto>>> GetUnlockedAchievementsForUser (Guid userId)
    {
        try
        {
            var achievements = await _achievementService.GetAchievementsForUserAsync(userId);

            if (achievements.Count == 0)
                return NoContent();
            
            return Ok(achievements);
        } catch (Exception ex)
        {
            return Problem($"Failed to get unlocked achievements: {ex.Message}");
        }
    }

    [HttpGet("stats/{userId}")]
    public async Task<ActionResult<UserStatsDto>> GetUserStats(Guid userId)
    {
        try
        {
            var stats = await _achievementService.GetUserStatsAsync(userId);

            return Ok(stats);
        } catch (Exception ex)
        {
            return Problem($"Failed to get user stats: {ex.Message}");
        }
    }
}