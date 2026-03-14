using Microsoft.AspNetCore.Mvc;
using psi25_project.Models.Dtos;
using psi25_project.Services.Interfaces;

namespace psi25_project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomsController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpPost("create")]
        public async Task<ActionResult<RoomDto>> CreateRoom([FromBody] RoomCreateDto dto)
        {
            var room = await _roomService.CreateRoomAsync(dto);
            return Ok(room);
        }

        [HttpPost("join")]
        public async Task<ActionResult<PlayerDto?>> JoinRoom([FromBody] JoinRoomDto dto)
        {
            var player = await _roomService.JoinRoomAsync(dto);
            if (player == null)
                return NotFound(new { message = "Room not found" });

            return Ok(player);
        }

        [HttpGet("{roomCode}/players")]
        public async Task<ActionResult<List<PlayerDto>>> GetPlayers(string roomCode)
        {
            var players = await _roomService.GetPlayersInRoomAsync(roomCode);
            return Ok(players);
        }
    }
}
