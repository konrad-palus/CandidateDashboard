using CandidateDashboardApi.Models;
using Microsoft.AspNetCore.Identity;

namespace CandidateDashboardApi.Interfaces
{
    public interface IAccountService
    {
        Task<ApiResponse<string>> RegisterUserAsync(RegistrationModel model);
        Task<string> GenerateJwtTokenAsync(string email);
        Task<ApiResponse<bool>> ConfirmUserEmailAsync(string email, string token);
        Task<ApiResponse<bool>> ForgotPasswordAsync(string email);
        Task<ApiResponse<bool>> ResetPasswordAsync(string email, string token, string password);
    }
}