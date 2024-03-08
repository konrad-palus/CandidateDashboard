using Domain.Entities;
using System.Security.Claims;

namespace CandidateDashboardApi.Interfaces
{
    public interface IUserService
    {
        public Task<object> GetUserDataAsync(ClaimsPrincipal userClaims);
        public Task<string> GetUserPhotoUrlAsync(string email);
        public Task<ApplicationUser> UpdateUserAsync(ClaimsPrincipal userClaims, string? name = null, string? lastName = null,
                                                           string? contactEmail = null, int? phoneNumber = null, string? city = null,
                                                                                                                string? country = null);
    }
}
