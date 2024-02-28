using System.Security.Claims;

namespace CandidateDashboardApi.Interfaces
{
    public interface IAccountService
    {
        public Task<string> RegisterUserAsync(RegistrationModel model);
        public Task<string> LoginUserAsync(string login, string password);
        public Task<string> GenerateJwtTokenAsync(string email);
        public Task<object> GetUserDataAsync(ClaimsPrincipal userClaims);
        public Task<bool> ConfirmUserEmailAsync(string email, string token);
        public Task<string> GetUserPhotoUrlAsync(string email);
    }
}