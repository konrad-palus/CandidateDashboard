using CandidateDashboardApi.Models;
using CandidateDashboardApi.Models.EmployerServiceModels;
using Domain.Entities;
using System.Security.Claims;

namespace CandidateDashboardApi.Interfaces
{
    public interface IUserService
    {
        Task<object> GetUserDataAsync(ClaimsPrincipal userClaims);
        Task<ApiResponse<string>> GetUserPhotoUrlAsync(string email);
        Task<ApplicationUser> UpdateUserAsync(ClaimsPrincipal userClaims, UserUpdateModel userUpdateModel);

    }
}
