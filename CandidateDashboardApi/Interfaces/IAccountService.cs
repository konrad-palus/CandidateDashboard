using System.Security.Claims;

namespace CandidateDashboardApi.Interfaces
{
    public interface IAccountService
    {
        public Task<string> RegisterUserAsync(string login, string registrationEmail, string password, bool isCandidate, string? name, string? lastName);
        public Task<string> LoginUserAsync(string login, string password);
        public Task<string> GenerateJwtTokenAsync(string email);
        public Task<object> GetUserDataAsync(ClaimsPrincipal userClaims);
    }
}