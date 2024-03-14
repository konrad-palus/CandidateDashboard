using AutoMapper;
using CandidateDashboardApi.Interfaces;
using CandidateDashboardApi.Models;
using CandidateDashboardApi.Models.EmployerServiceModels;
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
        [HttpPost("UploadPhoto")]
        public async Task<IActionResult> UploadPhoto([FromForm] IFormFile photo)
        {
            try
            {
                var response = await _blobService.UploadPhotoAsync(photo, User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value);
                return response.Success ? Ok(response) : BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(new List<string> { ex.Message }, $"Unexpected error occurred during uploading photo"));
            }
        }

        [Authorize]
        [HttpGet("GetPhoto")]
        public async Task<IActionResult> GetUserPhoto()
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            try
            {
                var response = await _userService.GetUserPhotoUrlAsync(userEmail!);
                return response.Success ? Ok(response) : BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(new List<string> { ex.Message }, $"Unexpected error occurred while getting photo"));
            }
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

        [Authorize]
        [HttpPost("UpdateUserDetails")]
        public async Task<IActionResult> UpdateUserData([FromBody] UserUpdateModel userUpdateModel)
        {
            try
            {
                var updatedUser = await _userService.UpdateUserAsync(User, userUpdateModel);
                var userResponse = _mapHelper.Map<UserDataModel>(updatedUser);

                return Ok(userResponse);
            }
            catch (InvalidOperationException ex)
            {

                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponseModel{ Message = "An unexpected error occurred" });
            }
        }
    }
}

