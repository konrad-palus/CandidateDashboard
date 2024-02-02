using CandidateDashboardApi.Interfaces;
using Domain.Entities;
using Domain.Entities.CandidateEntities;
using Microsoft.AspNetCore.Identity;
using Presistance;

namespace CandidateDashboardApi.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly CandidateDashboardContext _context;

        public AccountService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, CandidateDashboardContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public async Task<string> RegisterUserAsync(string login, string registrationEmail, string password, bool isCandidate, string? name, string? lastName)
        {
            var user = new ApplicationUser
            {
                UserName = login,
                Email = registrationEmail,
                Name = name,
                LastName = lastName,
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new System.Exception("Nie udało się zarejestrować użytkownika.");
            }

            if (isCandidate)
            {
                _context.Candidates.Add(new Candidate { Id = user.Id });
            }
            else
            {
                _context.Employers.Add(new Employer { Id = user.Id });
            }

            await _context.SaveChangesAsync();

            return user.Id;
        }

        public async Task<bool> LoginUserAsync(string login, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(login, password, isPersistent: true, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                throw new System.Exception("Logowanie nieudane.");
            }

            return true;
        }
    }
}