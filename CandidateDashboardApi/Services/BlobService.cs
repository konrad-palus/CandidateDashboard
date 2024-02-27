using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CandidateDashboardApi.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Presistance;

public class BlobService : IBlobService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName = "profilephoto";
    private readonly CandidateDashboardContext _candidateDashboardContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public BlobService(
        IConfiguration configuration,
        CandidateDashboardContext candidateDashboardContext,
        UserManager<ApplicationUser> userManager)
    {
        var connectionString = configuration.GetConnectionString("AzureBlobStorage");
        _blobServiceClient = new BlobServiceClient(connectionString);
        _candidateDashboardContext = candidateDashboardContext;
        _userManager = userManager;
    }

    public async Task<string> UploadPhotoAsync(IFormFile photo, string userEmail)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        string fileName = GetUniqueFileName(userEmail, photo.FileName);
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
        
        var user = await _userManager.FindByEmailAsync(userEmail);
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