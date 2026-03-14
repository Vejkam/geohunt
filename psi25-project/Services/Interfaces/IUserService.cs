using psi25_project.Models.Dtos;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace psi25_project.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<UserAccountDto>> GetAllUsersAsync();
        Task<UserAccountDto?> GetUserByIdAsync(Guid id);
        Task<UserAccountDto?> GetCurrentUserAsync(ClaimsPrincipal principal);
        Task<(bool Succeeded, IEnumerable<IdentityError>? Errors)> DeleteUserAsync(Guid id);
    }
}
