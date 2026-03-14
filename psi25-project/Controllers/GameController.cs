using Microsoft.AspNetCore.Mvc;
using psi25_project.Models;
using psi25_project.Models.Dtos;
using psi25_project.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;

    public GameController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpPost]
    public async Task<ActionResult<GameResponseDto>> StartGame([FromBody] CreateGameDto dto)
    {
        var gameDto = await _gameService.StartGameAsync(dto);
        return CreatedAtAction(nameof(GetGameById), new { id = gameDto.Id }, gameDto);
    }

    [HttpPatch("{id}/finish")]
    public async Task<ActionResult<Game>> FinishGame(Guid id)
    {
        try
        {
            var game = await _gameService.FinishGameAsync(id);
            return Ok(game);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Game not found");
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{id}/score")]
    public async Task<ActionResult<Game>> UpdateScore(Guid id, [FromBody] int score)
    {
        try
        {
            var game = await _gameService.UpdateScoreAsync(id, score);
            return Ok(game);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Game not found");
        }
    }

    [HttpGet("{id}/total-score")]
    public async Task<ActionResult<int>> GetTotalScore(Guid id)
    {
        try
        {
            var totalScore = await _gameService.GetTotalScoreAsync(id);
            return Ok(totalScore);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Game not found");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GameResponseDto>> GetGameById(Guid id)
    {
        var gameDto = await _gameService.GetGameByIdAsync(id);
        if (gameDto == null)
            return NotFound("Game not found");

        return Ok(gameDto);
    }
}
