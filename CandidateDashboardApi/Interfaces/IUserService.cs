using System.Security.Claims;

namespace CandidateDashboardApi.Interfaces
{
    public interface IUserService
    {
        public Task<object> GetUserDataAsync(ClaimsPrincipal userClaims);
        public Task<string> GetUserPhotoUrlAsync(string email);
    }
}
