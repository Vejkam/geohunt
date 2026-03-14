using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using psi25_project.Services.Interfaces;


[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userService.GetCurrentUserAsync(User);
        if (user == null) return Unauthorized("User is not logged in.");
        return Ok(user);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound("User not found.");
        return Ok(user);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var (succeeded, errors) = await _userService.DeleteUserAsync(id);
        if (!succeeded) return BadRequest(new { Errors = errors });

        return Ok(new { Message = $"User {id} deleted successfully" });
    }
}
