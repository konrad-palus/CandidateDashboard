namespace CandidateDashboardApi.Interfaces
{
    public interface IBlobService
    {
        public Task<string> UploadPhotoAsync(IFormFile photo, string userId);
    }
}
