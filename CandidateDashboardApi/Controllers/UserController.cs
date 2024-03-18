using AutoMapper;
using Azure;
using CandidateDashboardApi.Interfaces;
using CandidateDashboardApi.Models;
using CandidateDashboardApi.Models.EmployerServiceModels;
using CandidateDashboardApi.Models.ResponseModels.AccountServiceResponses;
using CandidateDashboardApi.Models.UserServiceModels;
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
        private readonly IUserService _userService;
        private readonly IMapper _mapHelper;
        public UserController(
            IBlobService blobService,
            IUserService userService,
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
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
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

        [Authorize]
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

        [Authorize]
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

