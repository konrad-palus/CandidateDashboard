using AutoMapper;
using CandidateDashboardApi.Interfaces;
using CandidateDashboardApi.Models;
using CandidateDashboardApi.Models.ResponseModels.AccountServiceResponses;
using CandidateDashboardApi.Models.UserServiceModels;
using CandidateDashboardApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CandidateDashboardApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IBlobService _blobService;
        private readonly UserService _userService;
        private readonly IMapper _mapHelper;
        public UserController(
            IBlobService blobService,
            UserService userService,
            IMapper mapHelper)
        {
            _blobService = blobService;
            _userService = userService;
            _mapHelper = mapHelper;
        }

        [Authorize]
        [HttpPost("upload-photo")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadPhoto([FromForm] IFormFile photo)
        {
            var photoUrl = await _blobService.UploadPhotoAsync(photo, User.FindFirstValue(ClaimTypes.NameIdentifier));

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

            var photoUrl = await _userService.GetUserPhotoUrlAsync(userEmail);
            if (string.IsNullOrEmpty(photoUrl))
            {
                return NotFound(new { message = "Photo not found." });
            }

            return Ok(new { photoUrl });
        }

        [Authorize]
        [HttpGet("GetUserData")]
        public async Task<IActionResult> GetUserData()
        {
            try
            {
                var userData = await _userService.GetUserDataAsync(User);
                return Ok(new GetUserDataResponse { UserData = userData, Message = "User data retrieval successful." });
            }
            catch (Exception ex)
            {
                return BadRequest(new GetUserDataResponse { Message = $"Failed to retrieve user data. {ex.Message}" });
            }
        }
#if DEPLOYMENT
        [Authorize]
#endif
        [HttpPost("UpdateUserDetails")]
        public async Task<IActionResult> UpdateUserData([FromBody] UserDataModel userDataModel)
        {
            try
            {
                var updatedUser = await _userService.UpdateUserAsync(User, userDataModel.Name, userDataModel.LastName, userDataModel.ContactEmail,
                    userDataModel.PhoneNumber, userDataModel.City, userDataModel.Country);

                var userResponse = _mapHelper.Map<UserDataModel>(updatedUser);

                return Ok(userResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponseModel { Message = $"Failed to update user details, {ex}" });
            }
        }
    }
}

