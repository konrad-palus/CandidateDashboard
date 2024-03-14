using CandidateDashboardApi.Models;
using Microsoft.AspNetCore.Identity;

namespace CandidateDashboardApi.Interfaces
{
    public interface IAccountService
    {
        Task<ApiResponse<string>> RegisterUserAsync(RegistrationModel model);
        Task<string> GenerateJwtTokenAsync(string email);
        Task<bool> ConfirmUserEmailAsync(string email, string token);
        Task ForgotPasswordAsync(string email);
        Task<IdentityResult> ResetPasswordAsync(string email, string token, string password);
    }
}