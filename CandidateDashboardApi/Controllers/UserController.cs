using CandidateDashboardApi.Interfaces;
using CandidateDashboardApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presistance;
using System.Security.Claims;

namespace CandidateDashboardApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IBlobService _blobService;
        private readonly AccountService _accountService;

        public UserController(
            IBlobService blobService,
            AccountService accountService)
        {
            _blobService = blobService;
            _accountService = accountService;
        }

        [HttpPost("upload-photo")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadPhoto([FromForm] IFormFile photo)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var photoUrl = await _blobService.UploadPhotoAsync(photo, userEmail);

            return Ok(new { photoUrl });
        }

        [Authorize]
        [HttpGet("get-photo")]
        public async Task<IActionResult> GetUserPhoto()
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized("User is not authenticated.");
            }

            var photoUrl = await _accountService.GetUserPhotoUrlAsync(userEmail);
            if (string.IsNullOrEmpty(photoUrl))
            {
                return NotFound(new { message = "Photo not found." });
            }

            return Ok(new { photoUrl });
        }
    }
}

