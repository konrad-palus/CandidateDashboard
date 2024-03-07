﻿using CandidateDashboardApi.Interfaces;
using Domain.Entities;
using Domain.Entities.CandidateEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;
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
        private readonly ILogger<AccountService> _logger;
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
            IActionContextAccessor actionContextAccessor,
             ILogger<AccountService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _candidateDashboardContext = context;
            _configuration = configuration;
            _emailService = emailService;
            _urlHelper = factory.GetUrlHelper(actionContextAccessor.ActionContext);
            _actionContextAccessor = actionContextAccessor;
            _logger = logger;
        }

        public async Task<string> RegisterUserAsync(RegistrationModel model)
        {
            _logger.LogInformation("Someone's trying to register account, email: {Email}", model.RegistrationEmail);

            var user = new ApplicationUser
            {
                UserName = model.Login,
                Email = model.RegistrationEmail,
                ContactEmail = model.RegistrationEmail,
                Name = model.Name,
                LastName = model.LastName,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                _logger.LogError("Registration failed: {Email}", model.RegistrationEmail);
                throw new Exception("Registration failed");
            }

            if (model.IsCandidate)
            {
                _logger.LogInformation("Registred Candidate: {Email}", model.RegistrationEmail);
                _candidateDashboardContext.Candidates.Add(new Candidate { Id = user.Id });
                await _userManager.AddToRoleAsync(user, nameof(Candidate));

            }
            else
            {
                _logger.LogInformation("Registred Employer: {Email}", model.RegistrationEmail);
                _candidateDashboardContext.Employers.Add(new Employer { Id = user.Id });
                await _userManager.AddToRoleAsync(user, nameof(Employer));
            }

            await _candidateDashboardContext.SaveChangesAsync();

            var token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await _userManager.GenerateEmailConfirmationTokenAsync(user)));

            var callbackUrl = _urlHelper.ActionLink(
                "ConfirmEmail", "Account",
                values: new { email = user.Email, token },
                protocol: _actionContextAccessor.ActionContext.HttpContext.Request.Scheme);

            await _emailService.SendEmailAsync(model.RegistrationEmail, "Confirm your email",
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
            _logger.LogInformation("Login attempt: {Login}", login);
            var user = await _userManager.FindByEmailAsync(login);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, password, isPersistent: true, lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                _logger.LogError("Login failed: {Login}", login);
                throw new Exception("Login failed");
            }

            return await GenerateJwtTokenAsync(user.Email);

        }

        public async Task<bool> ConfirmUserEmailAsync(string email, string token)
        {
            _logger.LogInformation("Attempting to confirm email for user: {Email}", email);
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogError("Email confirmation failed: user with email does not exist {Email}", email);
                throw new InvalidOperationException("User was null");
            }

            var result = await _userManager.ConfirmEmailAsync(user, Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token)));
            if (result.Succeeded)
            {
                _logger.LogInformation("Email successfully confirmed: {Email}", email);
            }
            else
            {
                _logger.LogError("Email confirmation failed: {Email}. Errors: {Errors}", email, string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return result.Succeeded;
        }

    }
}