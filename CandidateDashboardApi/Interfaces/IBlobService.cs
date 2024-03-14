using CandidateDashboardApi.Models;

namespace CandidateDashboardApi.Interfaces
{
    public interface IBlobService
    {
        Task<ApiResponse<string>> UploadPhotoAsync(IFormFile photo, string userEmail);
    }
}