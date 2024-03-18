using CandidateDashboardApi.Models;
using CandidateDashboardApi.Models.EmployerServiceModels;
using CandidateDashboardApi.Models.UserServiceModels;
using System.Security.Claims;

namespace CandidateDashboardApi.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<UserDataModel>> GetUserDataAsync(ClaimsPrincipal userClaims);
        Task<ApiResponse<string>> GetUserPhotoUrlAsync(ClaimsPrincipal userClaims);
        Task<ApiResponse<UserDataModel>> UpdateUserAsync(ClaimsPrincipal userClaims, UserDataModel userUpdateModel);
    }
}