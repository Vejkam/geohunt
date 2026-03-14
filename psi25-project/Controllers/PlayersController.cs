using Microsoft.AspNetCore.Mvc;
using psi25_project.Services.Interfaces;
using psi25_project.Models.Dtos;


namespace psi25_project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public PlayersController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpPost("{playerId}/toggle-ready")]
        public async Task<ActionResult<PlayerDto>> ToggleReady(Guid playerId)
        {
            var player = await _roomService.ToggleReadyAsync(playerId);
            if (player == null) return NotFound();

            var dto = new PlayerDto
            {
                Id = player.Id,
                UserId = player.UserId,
                DisplayName = player.DisplayName,
                IsReady = player.IsReady
            };

            return Ok(dto);
        }

        [HttpPost("{playerId}/ready")]
        public async Task<ActionResult<PlayerDto>> SetReady(Guid playerId)
        {
            var player = await _roomService.SetReadyAsync(playerId);
            if (player == null) return NotFound();

            var dto = new PlayerDto
            {
                Id = player.Id,
                UserId = player.UserId,
                DisplayName = player.DisplayName,
                IsReady = player.IsReady
            };

            return Ok(dto);
        }

        [HttpPost("{playerId}/leave-room")]
        public async Task<IActionResult> LeaveRoom(Guid playerId)
        {
            var success = await _roomService.LeaveRoomAsync(playerId);
            if (!success) return NotFound();

            return Ok();
        }
    }
}
