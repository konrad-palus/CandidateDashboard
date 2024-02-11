using CandidateDashboardApi.Interfaces;
using Domain.Entities;
using Domain.Entities.CandidateEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Presistance;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CandidateDashboardApi.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly CandidateDashboardContext _candidateDashboardContext;

        public AccountService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, CandidateDashboardContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _candidateDashboardContext = context;
        }

        public async Task<string> RegisterUserAsync(string login, string registrationEmail, string password, bool isCandidate, string? name, string? lastName)
        {
            var user = new ApplicationUser
            {
                UserName = login,
                Email = registrationEmail,
                ContactEmail = registrationEmail,
                Name = name,
                LastName = lastName,
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new Exception("registration failed");
            }

            if (isCandidate)
            {
                _candidateDashboardContext.Candidates.Add(new Candidate { Id = user.Id });
            }
            else
            {
                _candidateDashboardContext.Employers.Add(new Employer { Id = user.Id });
            }

            await _candidateDashboardContext.SaveChangesAsync();

            return user.Id;
        }

        public async Task<bool> LoginUserAsync(string login, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(login, password, isPersistent: true, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                throw new Exception("login failed");
            }

            return true;
        }
    }
}