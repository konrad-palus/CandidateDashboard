using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CandidateDashboardApi.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Presistance;
using System;
using System.IO;
using System.Threading.Tasks;

public class BlobService : IBlobService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName = "profilephoto";
    private readonly CandidateDashboardContext _candidateDashboardContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<BlobService> _logger;

    public BlobService(
        IConfiguration configuration,
        CandidateDashboardContext candidateDashboardContext,
        UserManager<ApplicationUser> userManager,
        ILogger<BlobService> logger) 
    {
        var connectionString = configuration.GetConnectionString("AzureBlobStorage");
        _blobServiceClient = new BlobServiceClient(connectionString);
        _candidateDashboardContext = candidateDashboardContext;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<string> UploadPhotoAsync(IFormFile photo, string userEmail)
    {
        _logger.LogInformation("Uploading photo for user: {UserEmail}", userEmail); 

        try
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

            _logger.LogInformation("Photo uploaded successfully for user: {UserEmail}", userEmail);

            return photoUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading photo for user: {UserEmail}", userEmail);
            throw;
        }
    }

    private string GetUniqueFileName(string userId, string fileName)
    {
        string fileExtension = Path.GetExtension(fileName);

        if (fileExtension != ".jpg" && fileExtension != ".png")
        {
            throw new InvalidOperationException("Invalid file type.");
        }

        return $"{userId}-{Guid.NewGuid()}{fileExtension}";
    }
}