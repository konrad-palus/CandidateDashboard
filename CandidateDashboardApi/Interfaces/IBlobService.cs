namespace CandidateDashboardApi.Interfaces
{
    public interface IBlobService
    {
         Task<string> UploadPhotoAsync(IFormFile photo, string userId);
    }
}
