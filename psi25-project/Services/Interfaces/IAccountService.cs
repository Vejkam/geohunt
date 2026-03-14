using psi25_project.Models.Dtos;
using Microsoft.AspNetCore.Identity;

namespace psi25_project.Services.Interfaces
{
    public interface IAccountService
    {
        Task<(bool Succeeded, IEnumerable<IdentityError>? Errors)> RegisterAsync(RegisterDto model);
        Task<(bool Succeeded, string? Error)> LoginAsync(LoginDto model);
        Task LogoutAsync();
        Task<(bool Succeeded, IEnumerable<string>? Errors)> AssignAdminAsync(Guid userId);
    }
}
