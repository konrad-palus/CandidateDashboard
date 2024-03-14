using AutoMapper;
using CandidateDashboardApi.Interfaces;
using CandidateDashboardApi.Models;
using CandidateDashboardApi.Models.EmployerServiceModels;
using CandidateDashboardApi.Models.UserServiceModels;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        //CRKE TU ZRÓB!!!
        public async Task<object> GetUserDataAsync(ClaimsPrincipal userClaims)
        {
            // var userEmail = userClaims.Claims.First(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var userEmail = "konradpalus@gmail.com";

            if (string.IsNullOrEmpty(userEmail))
            {
                _logger.LogError("User Email cannot be found in user claims.");
                throw new InvalidOperationException("User Email cannot be found.");
            }

            _logger.LogInformation($"Attempting to find user by Email: {userEmail}");
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                _logger.LogWarning($"User not found for Email: {userEmail}");
                throw new InvalidOperationException("User not found.");
            }

            _logger.LogInformation($"User found. Email: {userEmail}. Mapping to DTO...");

            var userDto = _mapper.Map<UserDataModel>(user);

            return userDto;
        }

        public async Task<ApiResponse<string>> GetUserPhotoUrlAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("{MethodName} -> User not found for email: {Email}", nameof(GetUserDataAsync), email);

                    return new ApiResponse<string>(new List<string> { "User not found" }, "User not found");
                }

                if (string.IsNullOrEmpty(user.PhotoUrl))
                {
                    _logger.LogInformation("{MethodName} -> Photo not found for user: {Email}", nameof(GetUserDataAsync), email);

                    return new ApiResponse<string>(new List<string> { "Photo not found" }, "Photo not found");
                }

                return new ApiResponse<string>(user.PhotoUrl, "Photo retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{MethodName} -> An error occurred while fetching the photo for user: {Email}", nameof(GetUserDataAsync), email);

                return new ApiResponse<string>(new List<string> { "An error occurred while fetching the photo" }, "An error occurred while fetching the photo");
            }
        }

        public async Task<ApplicationUser> UpdateUserAsync(ClaimsPrincipal userClaims, UserUpdateModel userUpdateModel)
        {
            var userEmail = userClaims.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                throw new InvalidOperationException("User email not found in claims.");
            }

            var user = await _userManager.FindByEmailAsync(userEmail);

            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            _mapper.Map(userUpdateModel, user);

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogError("User update failed: {errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                throw new InvalidOperationException($"User update failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            _logger.LogInformation("User updated successfully.");

            return user;
        }
    }
}