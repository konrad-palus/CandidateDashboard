using CandidateDashboardApi.Interfaces;
using Domain.Entities;
using Domain.Entities.CandidateEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Presistance;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using WebApi.Services.Interfaces;

namespace CandidateDashboardApi.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly CandidateDashboardContext _candidateDashboardContext;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IUrlHelper _urlHelper;
        private readonly IActionContextAccessor _actionContextAccessor;


        public AccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            CandidateDashboardContext context,
            IConfiguration configuration,
            IEmailService emailService,
            IUrlHelperFactory factory,
            IActionContextAccessor actionContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _candidateDashboardContext = context;
            _configuration = configuration;
            _emailService = emailService;
            _urlHelper = factory.GetUrlHelper(actionContextAccessor.ActionContext);
            _actionContextAccessor = actionContextAccessor;
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
                throw new Exception("Registration failed");
            }

            if (isCandidate)
            {
                _candidateDashboardContext.Candidates.Add(new Candidate { Id = user.Id });
                await _userManager.AddToRoleAsync(user, nameof(Candidate));

            }
            else
            {
                _candidateDashboardContext.Employers.Add(new Employer { Id = user.Id });
                await _userManager.AddToRoleAsync(user, nameof(Employer));
            }

            await _candidateDashboardContext.SaveChangesAsync();

            var token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await _userManager.GenerateEmailConfirmationTokenAsync(user)));

            var callbackUrl = _urlHelper.ActionLink(
                "ConfirmEmail", "Account",
                values: new { email = user.Email, token },
                protocol: _actionContextAccessor.ActionContext.HttpContext.Request.Scheme);

            await _emailService.SendEmailAsync(registrationEmail, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

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
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                 new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                 new Claim(ClaimTypes.NameIdentifier, user.Id),
                 
            }
            .Union(userClaims).ToList();

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

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

        public async Task<bool> ConfirmUserEmailAsync(string email, string token)
        {

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new InvalidOperationException("user was null");
            }

            var result = await _userManager.ConfirmEmailAsync(user, Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token)));
            return result.Succeeded;
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
        public async Task<string> GetUserPhotoUrlAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new Exception("User not found.");
            }

            return user.PhotoUrl;
        }
    }
}