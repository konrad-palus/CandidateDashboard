using CandidateDashboardApi.Interfaces;
using Domain.Entities;
using Domain.Entities.CandidateEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Presistance;
using System.Data.Entity;
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
        private readonly IConfiguration _configuration;
        public AccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            CandidateDashboardContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _candidateDashboardContext = context;
            _configuration = configuration;
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

        public async Task<string> GenerateJwtTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            var userClaims = await _userManager.GetClaimsAsync(user);

            var claims = new List<Claim>
            {
                 new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                 new Claim(ClaimTypes.NameIdentifier, user.Id),
            }
            .Union(userClaims);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddHours(2);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> LoginUserAsync(string login, string password)
        {
            var user = await _userManager.FindByNameAsync(login);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, password, isPersistent: true, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                throw new Exception("Login failed");
            }

            return await GenerateJwtTokenAsync(user.Email);

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
                    Id = candidate.Id,
                    About = candidate.About,
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
                    Id = employer.Id,
                    CompanyName = employer.CompanyName,
                    CompanyLogo = employer.CompanyLogo,
                    CompanyDescription = employer.CompanyDescription,
                    Sites = employer.ImportantSites
                };
            }

            throw new Exception("Something went wrong  GetUserDataAsync");
        }
    }
}