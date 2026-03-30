using Microsoft.AspNetCore.Identity;
using psi25_project.Models;
using psi25_project.Models.Dtos;
using psi25_project.Services.Interfaces;

namespace psi25_project.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<(bool Succeeded, IEnumerable<IdentityError>? Errors, string? TwoFactorToken)> RegisterAsync(RegisterDto model)
        {
            if (model.EnableTwoFactor)
            {
                model.TwoFactorProvider = "Email";
            }

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return (false, result.Errors, null);

            if (model.EnableTwoFactor)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, true);
                var twoFactorToken = await _userManager.GenerateTwoFactorTokenAsync(user, model.TwoFactorProvider);
                await _userManager.AddToRoleAsync(user, "Player");
                return (true, null, twoFactorToken);
            }

            await _userManager.AddToRoleAsync(user, "Player");
            return (true, null, null);
        }

        public async Task<(bool Succeeded, string? Error, bool RequiresTwoFactor, string? TwoFactorProvider)> LoginAsync(LoginDto model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, isPersistent: false, lockoutOnFailure: false);
            if (result.Succeeded)
                return (true, null, false, null);

            if (result.RequiresTwoFactor)
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                string? provider = null;
                if (user != null)
                {
                    var providers = await _userManager.GetValidTwoFactorProvidersAsync(user);
                    provider = providers.Count > 0 ? providers[0] : null;
                }

                return (false, "Two-factor authentication required.", true, provider);
            }

            return (false, "Invalid username or password.", false, null);
        }

        public async Task<(bool Succeeded, string? Error)> VerifyTwoFactorAsync(TwoFactorDto model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
                return (false, "Invalid username or two-factor code.");

            if (!await _userManager.GetTwoFactorEnabledAsync(user))
                return (false, "Two-factor authentication is not enabled for this user.");

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, model.Provider, model.Code);
            if (!isValid)
                return (false, "Invalid two-factor code.");

            await _signInManager.SignInAsync(user, isPersistent: false);
            return (true, null);
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<(bool Succeeded, IEnumerable<string>? Errors)> AssignAdminAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return (false, new[] { "User not found" });

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
                await _userManager.AddToRoleAsync(user, "Admin");

            return (true, null);
        }
    }
}
