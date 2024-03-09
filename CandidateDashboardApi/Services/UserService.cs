using AutoMapper;
using CandidateDashboardApi.Interfaces;
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

        public async Task<string> GetUserPhotoUrlAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            return user.PhotoUrl;
        }

        public async Task<ApplicationUser> UpdateUserAsync(ClaimsPrincipal userClaims, string? name = null, string? lastName = null,
                                                           string? contactEmail = null, int? phoneNumber = null, string? city = null,
                                                                                                                string? country = null)
        {
            var user = await _userManager.FindByEmailAsync(userClaims.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            user.Name = name ?? user.Name;
            user.LastName = lastName ?? user.LastName;
            user.ContactEmail = contactEmail ?? user.ContactEmail;
            user.PhoneNumber = phoneNumber ?? user.PhoneNumber;
            user.City = city ?? user.City;
            user.Country = country ?? user.Country;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                _logger.LogError("User update failed: {errors}", result.Errors);
                throw new InvalidOperationException("User update failed.");
            }

            _logger.LogInformation("Uesr updated");
            return user;
        }
    }
}
