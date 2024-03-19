using AutoMapper;
using CandidateDashboardApi.Interfaces;
using CandidateDashboardApi.Models.UserServiceModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CandidateDashboardApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IBlobService _blobService;
        private readonly IUserService _userService;
        public UserController(
            IBlobService blobService,
            IUserService userService)
        {
            _blobService = blobService;
            _userService = userService;
        }

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
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetPhoto")]
        public async Task<IActionResult> GetUserPhoto()
        {
            try
            {
                var userPhoto = await _userService.GetUserPhotoUrlAsync(User);
                return userPhoto.Success ? Ok(userPhoto) : BadRequest(userPhoto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetUserData")]
        public async Task<IActionResult> GetUserData()
        {
            try
            {
                var userData = await _userService.GetUserDataAsync(User);
                return userData.Success ? Ok(userData) : BadRequest(userData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateUserDetails")]
        public async Task<IActionResult> UpdateUserData([FromBody] UserDataModel userUpdateModel)
        {
            try
            {
                var userDetails = await _userService.UpdateUserAsync(User, userUpdateModel);
                return userDetails.Success ? Ok(userDetails) : BadRequest(userDetails);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}