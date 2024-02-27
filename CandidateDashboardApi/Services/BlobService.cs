using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CandidateDashboardApi.Interfaces;
using Microsoft.EntityFrameworkCore;
using Presistance;

public class BlobService : IBlobService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName = "profilephoto";
    private readonly CandidateDashboardContext _candidateDashboardContext;

    public BlobService(IConfiguration configuration, CandidateDashboardContext candidateDashboardContext)
    {
        var connectionString = configuration.GetConnectionString("AzureBlobStorage");
        _blobServiceClient = new BlobServiceClient(connectionString);
        _candidateDashboardContext = candidateDashboardContext;
    }

    public async Task<string> UploadPhotoAsync(IFormFile photo, string userId)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        string fileName = GetUniqueFileName(userId, photo.FileName);
        var blobClient = containerClient.GetBlobClient(fileName);

        if (await blobClient.ExistsAsync())
        {
            await blobClient.DeleteAsync();
        }

        using (var stream = photo.OpenReadStream())
        {
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = photo.ContentType });
        }

        var photoUrl = blobClient.Uri.ToString();

        var user = await _candidateDashboardContext.Users.SingleOrDefaultAsync(u => u.Id == userId);
        if (user != null)
        {
            user.PhotoUrl = photoUrl;
            _candidateDashboardContext.Users.Update(user);
            await _candidateDashboardContext.SaveChangesAsync();
        }

        return photoUrl;
    }

    private string GetUniqueFileName(string userId, string fileName)
    {
        string fileExtension = Path.GetExtension(fileName);
        if (fileExtension != ".jpg" && fileExtension != ".png")
            throw new InvalidOperationException("Invalid file type.");

        return $"{userId}-{Guid.NewGuid()}{fileExtension}";
    }
}