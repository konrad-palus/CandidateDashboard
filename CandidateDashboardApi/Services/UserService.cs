﻿using CandidateDashboardApi.Interfaces;
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
        private readonly ILogger<AccountService> _logger;
        public UserService(CandidateDashboardContext candidateDashboardContext,
             UserManager<ApplicationUser> userManager,
             ILogger<AccountService> logger)
        {
            _candidateDashboardContext = candidateDashboardContext;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<object> GetUserDataAsync(ClaimsPrincipal userClaims)
        {
            var userId = userClaims.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

            var candidate = await _candidateDashboardContext.Candidates
                .Include(c => c.CandidateEducations)
                .Include(c => c.CandidateExperience)
                .Include(c => c.CandidateSkills)
                .Include(c => c.ImportantSites)
                .Include(c => c.CandidateJobWanted)
                .FirstOrDefaultAsync(c => c.Id == userId);

            if (candidate != null)
            {
                return new
                {
                    candidate.Id,
                    candidate.About,
                    Educations = candidate.CandidateEducations,
                    Experiences = candidate.CandidateExperience,
                    Skills = candidate.CandidateSkills,
                    Sites = candidate.ImportantSites,
                    JobsWanted = candidate.CandidateJobWanted
                };
            }

            var employer = await _candidateDashboardContext.Employers
                .Include(e => e.ImportantSites)
                .FirstOrDefaultAsync(e => e.Id == userId);

            if (employer != null)
            {
                return new
                {
                    employer.Id,
                    employer.CompanyName,
                    employer.CompanyDescription,
                    Sites = employer.ImportantSites
                };
            }

            throw new Exception("Something went wrong  GetUserDataAsync");
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