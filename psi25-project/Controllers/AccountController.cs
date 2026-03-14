using Microsoft.AspNetCore.Mvc;
using psi25_project.Models.Dtos;
using psi25_project.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace psi25_project.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (succeeded, errors) = await _accountService.RegisterAsync(model);

            if (!succeeded)
                return BadRequest(errors);

            return Ok(new { Message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (succeeded, error) = await _accountService.LoginAsync(model);

            if (!succeeded)
                return Unauthorized(new { Message = error });

            return Ok(new { Message = "User logged in successfully." });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutAsync();
            return Ok(new { Message = "User logged out successfully." });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("assign-admin")]
        public async Task<IActionResult> AssignAdmin([FromBody] AssignRoleDto model)
        {
            var (succeeded, errors) = await _accountService.AssignAdminAsync(model.UserId);

            if (!succeeded)
                return BadRequest(new { Errors = errors });

            return Ok(new { Message = $"User with ID {model.UserId} is now an Admin" });
        }
    }
}
