using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CandidateDashboardApi.Interfaces;
using CandidateDashboardApi.Models;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Presistance;

public class BlobService : IBlobService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    private readonly CandidateDashboardContext _candidateDashboardContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<BlobService> _logger;

    public BlobService(
        IConfiguration configuration,
        CandidateDashboardContext candidateDashboardContext,
        UserManager<ApplicationUser> userManager,
        ILogger<BlobService> logger)
    {
        _containerName = configuration["AppSettings:BlobContainerName"]!;

        _blobServiceClient = new BlobServiceClient(configuration.GetConnectionString("AzureBlobStorage"));
        _candidateDashboardContext = candidateDashboardContext;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<ApiResponse<string>> UploadPhotoAsync(IFormFile photo, string userEmail)
    {
        _logger.LogInformation("{MethodName} -> Attempting to upload photo for user: {UserEmail}", nameof(UploadPhotoAsync), userEmail);

        if (photo == null)
        {
            _logger.LogWarning("{MethodName} -> Photo upload failed for user: {UserEmail} because the file was empty.", nameof(UploadPhotoAsync), userEmail);

            return new ApiResponse<string>("No photo uploaded, the file was empty.", "No photo uploaded, the file was empty.");
        }

        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        var blobClient = containerClient.GetBlobClient(GetUniqueFileName(userEmail, photo.FileName));

        if (await blobClient.ExistsAsync())
        {
            await blobClient.DeleteIfExistsAsync();
        }

        try
        {
            using (var stream = photo.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = photo.ContentType });
            }

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                _logger.LogWarning("{MethodName} -> Photo upload failed for user: {UserEmail} because the user was not found.", nameof(UploadPhotoAsync), userEmail);

                return new ApiResponse<string>("User not found.", "User not found.");
            }

            user.PhotoUrl = blobClient.Uri.ToString();
            _candidateDashboardContext.Users.Update(user);
            await _candidateDashboardContext.SaveChangesAsync();

            _logger.LogInformation("{MethodName} -> Photo uploaded successfully for user: {UserEmail}", nameof(UploadPhotoAsync), userEmail);

            return new ApiResponse<string>(user.PhotoUrl, "Photo uploaded successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{MethodName} -> Error occurred uploading photo for user: {UserEmail}", nameof(UploadPhotoAsync), userEmail);

            return new ApiResponse<string>(ex.Message, $"Error occurred while uploading the photo: {ex.Message}");
        }
    }

    private string GetUniqueFileName(string userId, string fileName)
    {
        string fileExtension = Path.GetExtension(fileName);

        if (fileExtension != ".jpg" && fileExtension != ".png")
        {
            _logger.LogError("{MethodName} -> Error occurred while getting unique file name", nameof(GetUniqueFileName));
            throw new InvalidOperationException("Invalid file type.");
        }

        return $"{userId}-{Guid.NewGuid()}{fileExtension}";
    }
}