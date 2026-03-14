using Microsoft.AspNetCore.Identity;
using psi25_project.Models;
using psi25_project.Models.Dtos;
using psi25_project.Repositories;
using psi25_project.Services.Interfaces;
using System.Security.Claims;

namespace psi25_project.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepository _userRepository;

        public UserService(UserManager<ApplicationUser> userManager, IUserRepository userRepository)
        {
            _userManager = userManager;
            _userRepository = userRepository;
        }

        public async Task<List<UserAccountDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            var result = new List<UserAccountDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(MapToDto(user, roles));
            }

            return result;
        }

        public async Task<UserAccountDto?> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return MapToDto(user, roles);
        }

        public async Task<UserAccountDto?> GetCurrentUserAsync(ClaimsPrincipal principal)
        {
            var username = principal.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return null;

            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return MapToDto(user, roles);
        }

        public async Task<(bool Succeeded, IEnumerable<IdentityError>? Errors)> DeleteUserAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return (false, new[] { new IdentityError { Description = "User not found" } });

            var result = await _userManager.DeleteAsync(user);
            return (result.Succeeded, result.Errors);
        }

        private static UserAccountDto MapToDto(ApplicationUser user, IEnumerable<string> roles)
        {
            return new UserAccountDto
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                Roles = roles.ToList()
            };
        }
    }
}
