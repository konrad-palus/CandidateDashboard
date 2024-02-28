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

        public EmployerService(UserManager<ApplicationUser> userManager, CandidateDashboardContext candidateDashboardContext)
        {
            _userManager = userManager;
            _candidateDashboardContext = candidateDashboardContext;
        }
        public async Task<string> UpdateOrCreateCompanyName(string userEmail, string companyName)
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

            return $"CompanyName updated to {companyName}";
        }

        public async Task<string> UpdateOrCreateCompanyDescription(string userEmail, string companyDescription)
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

            return $"CompanyDescription updated to {companyDescription}";
        }
    }
}