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

        public EmployerService(
            UserManager<ApplicationUser> userManager,
            CandidateDashboardContext candidateDashboardContext,
            ILogger<EmployerService> logger)
        {
            _userManager = userManager;
            _candidateDashboardContext = candidateDashboardContext;
            _logger = logger;
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

                var employer = await _candidateDashboardContext.Employers.FirstOrDefaultAsync(e => e.Id == user.Id);
                if (employer == null)
                {
                    employer = new Employer { Id = user.Id, CompanyName = companyName };
                    _candidateDashboardContext.Employers.Add(employer);
                }
                else
                {
                    employer.CompanyName = companyName;
                    _candidateDashboardContext.Employers.Update(employer);
                }

                await _candidateDashboardContext.SaveChangesAsync();

                _logger.LogInformation("Company name updated or created successfully for user: {UserEmail}", userEmail);
                return companyName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating or creating company name for user: {UserEmail}", userEmail);
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

                var employer = await _candidateDashboardContext.Employers.FirstOrDefaultAsync(e => e.Id == user.Id);
                if (employer == null)
                {
                    employer = new Employer { Id = user.Id, CompanyDescription = companyDescription };
                    _candidateDashboardContext.Employers.Add(employer);
                }
                else
                {
                    employer.CompanyDescription = companyDescription;
                    _candidateDashboardContext.Employers.Update(employer);
                }

                await _candidateDashboardContext.SaveChangesAsync();

                _logger.LogInformation("Company description updated or created successfully for user: {UserEmail}", userEmail);
                return companyDescription;
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
                throw new Exception("User not found");
            }

            var employer = await _candidateDashboardContext.Employers.FirstOrDefaultAsync(e => e.Id == user.Id);

            return employer.CompanyName ?? "Company name not set";
        }

        public async Task<string> GetCompanyDescriptionAsync(string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var employer = await _candidateDashboardContext.Employers.FirstOrDefaultAsync(e => e.Id == user.Id);

            return employer.CompanyDescription ?? "Company description not set";
        }
    }
}
