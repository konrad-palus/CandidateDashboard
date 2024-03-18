using AutoMapper;
using CandidateDashboardApi.Interfaces;
using CandidateDashboardApi.Models;
using CandidateDashboardApi.Models.EmployerServiceModels;
using CandidateDashboardApi.Models.UserServiceModels;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Presistance;
using System.Security.Claims;

namespace CandidateDashboardApi.Services
{
    public class UserService : IUserService
    {
        private readonly CandidateDashboardContext _candidateDashboardContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserService> _logger;
        private readonly IMapper _mapper;
        private readonly string _basePhotoUrl = "https://candidatedashboardstorag.blob.core.windows.net/profilephoto/cat-its-mouth-open.jpg";

        public UserService(CandidateDashboardContext candidateDashboardContext,
             UserManager<ApplicationUser> userManager,
             ILogger<UserService> logger,
             IMapper mapper)
        {
            _candidateDashboardContext = candidateDashboardContext;
            _userManager = userManager;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ApiResponse<UserDataModel>> GetUserDataAsync(ClaimsPrincipal userClaims)
        {
            var userEmail = userClaims.Claims.First(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                _logger.LogError("{MethodName} User Email cannot be found in user claims.", nameof(GetUserDataAsync));
                var errorResponse = new ApiResponse<UserDataModel>(data: null, message: "User Email cannot be found.");
                errorResponse.Success = false;
                errorResponse.Errors = new List<string> { "User Email cannot be found in user claims." };
                return errorResponse;
            }

            _logger.LogInformation("{MethodName} Attempting to find user by Email: {userEmail}", nameof(GetUserDataAsync), userEmail);
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                _logger.LogWarning($"User not found for Email: {userEmail}");
                var notFoundResponse = new ApiResponse<UserDataModel>(data: null, message: "User not found.");
                notFoundResponse.Success = false;
                notFoundResponse.Errors = new List<string> { $"User not found for Email: {userEmail}" };
                return notFoundResponse;
            }

            _logger.LogInformation("{MethodName} User found. Email: {userEmail}.", nameof(GetUserDataAsync), userEmail);

            var userDto = _mapper.Map<UserDataModel>(user);
            return new ApiResponse<UserDataModel>(data: userDto, message: "User data retrieved successfully.");
        }

        public async Task<ApiResponse<string>> GetUserPhotoUrlAsync(ClaimsPrincipal userClaims)
        {
            var userEmail = userClaims.Claims.First(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            try
            {
                var user = await _userManager.FindByEmailAsync(userEmail);
                if (user == null)
                {
                    _logger.LogWarning("{MethodName} -> User not found for email: {Email}", nameof(GetUserDataAsync), userEmail);

                    return new ApiResponse<string>(new List<string> { "User not found" }, "User not found");
                }

                if (string.IsNullOrEmpty(user.PhotoUrl))
                {
                    _logger.LogInformation("{MethodName} -> Photo not found for user: {Email}", nameof(GetUserDataAsync), userEmail);

                    return new ApiResponse<string>(new List<string> { "Photo not found" }, "Photo not found");
                }

                return new ApiResponse<string>(user.PhotoUrl, "Photo retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{MethodName} -> An error occurred while fetching the photo for user: {Email}", nameof(GetUserDataAsync), userEmail);

                return new ApiResponse<string>(new List<string> { "An error occurred while fetching the photo" }, "An error occurred while fetching the photo");
            }
        }

        public async Task<ApiResponse<UserDataModel>> UpdateUserAsync(ClaimsPrincipal userClaims, UserDataModel userUpdateModel)
        {
            var methodName = nameof(UpdateUserAsync);
            var userEmail = userClaims.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                _logger.LogError("{MethodName} User email not found in claims.", methodName);
                return new ApiResponse<UserDataModel>(data: null, message: "User email not found in claims.")
                { Success = false, Errors = new List<string> { "User email not found in claims." } };
            }

            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                _logger.LogError("{MethodName} User not found.", methodName);
                return new ApiResponse<UserDataModel>(data: null, message: "User not found.")
                { Success = false, Errors = new List<string> { "User not found." } };
            }

            _mapper.Map(userUpdateModel, user);
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogError("{MethodName} User update failed: {Errors}", methodName, string.Join(", ", result.Errors.Select(e => e.Description)));
                return new ApiResponse<UserDataModel>(data: null, message: "User update failed.")
                { Success = false, Errors = result.Errors.Select(e => e.Description).ToList() };
            }

            _logger.LogInformation("Mapping ApplicationUser to UserDataModel");
            var updatedUserDto = _mapper.Map<UserDataModel>(user);
            _logger.LogInformation("{MethodName} User updated successfully.", methodName);
            return new ApiResponse<UserDataModel>(data: updatedUserDto, message: "User updated successfully.");
        }
    }
}