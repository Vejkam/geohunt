using psi25_project.Models.Dtos;
using Microsoft.AspNetCore.Identity;

namespace psi25_project.Services.Interfaces
{
    public interface IAccountService
    {
        Task<(bool Succeeded, IEnumerable<IdentityError>? Errors, string? TwoFactorToken)> RegisterAsync(RegisterDto model);
        Task<(bool Succeeded, string? Error, bool RequiresTwoFactor, string? TwoFactorProvider)> LoginAsync(LoginDto model);
        Task<(bool Succeeded, string? Error)> VerifyTwoFactorAsync(TwoFactorDto model);
        Task LogoutAsync();
        Task<(bool Succeeded, IEnumerable<string>? Errors)> AssignAdminAsync(Guid userId);
    }
}
