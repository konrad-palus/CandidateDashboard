using CandidateDashboardApi.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Presistance;

namespace CandidateDashboardApi.Services
{
    public class EmployerService : IEmployerService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CandidateDashboardContext _candidateDashboardContext;
        private readonly ILogger<EmployerService> _logger;
        private readonly IOpenAIService _openAIService;

        public EmployerService(
            UserManager<ApplicationUser> userManager,
            CandidateDashboardContext candidateDashboardContext,
            ILogger<EmployerService> logger,
            IOpenAIService openAIService)
        {
            _userManager = userManager;
            _candidateDashboardContext = candidateDashboardContext;
            _logger = logger;
            _openAIService = openAIService;
        }
        public async Task<string> UpdateOrCreateCompanyNameAsync(string userEmail, string companyName)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(userEmail);
                if (user == null)
                {
                    throw new Exception("User not found");
                }

              
                if (user is Employer employer)
                {
                    employer.CompanyName = companyName;
                    await _userManager.UpdateAsync(employer);

                    _logger.LogInformation("Company name updated successfully for user: {UserEmail}", userEmail);
                    return companyName;
                }
                else
                {
                    throw new Exception("User is not an employer");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating company name for user: {UserEmail}", userEmail);
                throw;
            }
        }

        public async Task<string> UpdateOrCreateCompanyDescriptionAsync(string userEmail, string companyDescription)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(userEmail);
                if (user == null)
                {
                    throw new Exception("User not found");
                }

                if (user is Employer employer)
                {
                    employer.CompanyDescription = companyDescription;
                    var result = await _userManager.UpdateAsync(employer);
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Failed to update company description for {userEmail}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }

                    _logger.LogInformation("Company description updated or created successfully for user: {UserEmail}", userEmail);
                    return companyDescription;
                }
                else
                {
                    throw new Exception("User is not an employer");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating or creating company description for user: {UserEmail}", userEmail);
                throw;
            }
        }

        public async Task<string> GetCompanyNameAsync(string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                _logger.LogError("{MethodName}: User not found for email {UserEmail}", nameof(GetCompanyNameAsync), userEmail);
                return "User not found";
            }

            var employer = user as Employer; 
            return employer?.CompanyName ?? "Company name is empty";
        }

        public async Task<string> GetCompanyDescriptionAsync(string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                _logger.LogError("{MethodName}: User not found for email {UserEmail}", nameof(GetCompanyDescriptionAsync), userEmail);
                return "User not found";
            }

            var employer = await _candidateDashboardContext.Employers.FirstOrDefaultAsync(e => e.Id == user.Id);
            return employer?.CompanyDescription ?? "Company description is empty";
        }

        public async Task<string> GenerateAndUpdateCompanyDescriptionAsync(string userEmail)
        {
            try
            {
                var companyName = "Generate company description for company:" + await GetCompanyNameAsync(userEmail);

                if (companyName == "Company name is empty")
                {
                    _logger.LogWarning("Attempt to generate company description for user: {UserEmail} without a set company name", userEmail);
                    throw new InvalidOperationException("Company name must be established before generating company description.");
                }

                var chatResponse = await _openAIService.GetChatResponseAsync(companyName);

                if (chatResponse.Content == "No response received." || chatResponse.Content.Contains("An error occurred"))
                {
                    _logger.LogError("Failed to generate company description for {CompanyName}", companyName);
                    throw new Exception("Failed to generate company description");
                }

                var updatedDescription = await UpdateOrCreateCompanyDescriptionAsync(userEmail, chatResponse.Content);
                _logger.LogInformation("Company description generated and updated successfully for {CompanyName}.", companyName);
                return updatedDescription;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating or updating company description for user: {UserEmail}", userEmail);
                throw;
            }
        }
    }
}
