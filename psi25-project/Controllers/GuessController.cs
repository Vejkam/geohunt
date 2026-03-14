using Microsoft.AspNetCore.Mvc;
using psi25_project.Models.Dtos;
using psi25_project.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class GuessController : ControllerBase
{
    private readonly IGuessService _guessService;

    public GuessController(IGuessService guessService)
    {
        _guessService = guessService;
    }

    [HttpPost]
    public async Task<ActionResult> CreateGuess([FromBody] CreateGuessDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var (guessDto, finished, currentRound, totalScore, newAchievements) = await _guessService.CreateGuessAsync(dto);

            return CreatedAtAction(nameof(CreateGuess), new {id = guessDto.Id}, new
            {
                guess = guessDto,
                finished,
                currentRound,
                totalScore,
                achievementsUnlocked = newAchievements
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{gameId}")]
    public async Task<ActionResult<IEnumerable<GuessResponseDto>>> GetGuessesForGame(Guid gameId)
    {
        var guesses = await _guessService.GetGuessesForGameAsync(gameId);
        return Ok(guesses);
    }
}
