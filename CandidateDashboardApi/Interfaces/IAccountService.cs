using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace CandidateDashboardApi.Interfaces
{
    public interface IAccountService
    {
        public Task<string> RegisterUserAsync(RegistrationModel model);
        public Task<string> LoginUserAsync(string login, string password);
        public Task<string> GenerateJwtTokenAsync(string email);
        public Task<bool> ConfirmUserEmailAsync(string email, string token);
        public Task ForgotPasswordAsync(string email);
        public Task<IdentityResult> ResetPasswordAsync(string email, string token, string password);
    }
}