using AutoMapper;
using CandidateDashboardApi.Interfaces;
using CandidateDashboardApi.Models;
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
        private readonly IMapper _mapper;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            CandidateDashboardContext context,
            IConfiguration configuration,
            IEmailService emailService,
            IUrlHelperFactory factory,
            IActionContextAccessor actionContextAccessor,
             ILogger<AccountService> logger,
              IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _candidateDashboardContext = context;
            _configuration = configuration;
            _emailService = emailService;
            _urlHelper = factory.GetUrlHelper(actionContextAccessor.ActionContext);
            _actionContextAccessor = actionContextAccessor;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ApiResponse<string>> RegisterUserAsync(RegistrationModel model)
        {
            _logger.LogInformation("{MethodName} -> Starting registration process for email: {Email}", nameof(RegisterUserAsync),  model.RegistrationEmail);

            var existingUser = await _userManager.FindByEmailAsync(model.RegistrationEmail);
            if (existingUser != null)
            {
                _logger.LogError("{MethodName} -> Registration failed. User with email: {Email} already exists.", nameof(RegisterUserAsync), model.RegistrationEmail);
                return new ApiResponse<string>(new List<string> { $"User with email {model.RegistrationEmail} already exists." }, "User already exists");
            }

            ApplicationUser user = model.IsCandidate ? _mapper.Map<Candidate>(model) : _mapper.Map<Employer>(model);

            var createUserResult = await _userManager.CreateAsync(user, model.Password);
            if (!createUserResult.Succeeded)
            {
                _logger.LogError("{MethodName} -> Registration failed for: {Email}. Errors: {Errors}", nameof(RegisterUserAsync), model.RegistrationEmail,
                                    string.Join(", ", createUserResult.Errors.Select(e => e.Description)));
                return new ApiResponse<string>(createUserResult.Errors.Select(e => e.Description).ToList(), "Registration failed");
            }

            var roleResult = await _userManager.AddToRoleAsync(user, model.IsCandidate ? nameof(Candidate) : nameof(Employer));
            if (!roleResult.Succeeded)
            {
                _logger.LogError("{MethodName} -> Failed to add user to role for: {Email}.", nameof(RegisterUserAsync), model.RegistrationEmail);
                return new ApiResponse<string>(roleResult.Errors.Select(e => e.Description).ToList(), "Failed to add user to role");
            }

            var confirmationToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await _userManager.GenerateEmailConfirmationTokenAsync(user)));
            var callbackUrl = _urlHelper.ActionLink("ConfirmEmail", "Account",
                                                        new { email = user.Email, token = confirmationToken },
                                                        protocol: _actionContextAccessor.ActionContext!.HttpContext.Request.Scheme);

            await _emailService.SendEmailAsync(model.RegistrationEmail,
                                                "Confirm your email", $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>.");

            _logger.LogInformation("{MethodName} -> User registration completed successfully for: {Email}", nameof(RegisterUserAsync), model.RegistrationEmail);
            return new ApiResponse<string>(user.Id, "Registration successful");
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
            _logger.LogInformation("Login attempt: {Login}", login);

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

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                var token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await _userManager.GeneratePasswordResetTokenAsync(user)));
                //test
                var callbackUrl = $"http://localhost:4200/welcome/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";
                _logger.LogInformation("User {email} changed password", email);


                await _emailService.SendEmailAsync(user.Email, "Reset hasła",
                    $"Aby zresetować hasło kliknij <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>tutaj</a>.");
            }
        }

        public async Task<IdentityResult> ResetPasswordAsync(string email, string token, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                _logger.LogWarning($"{nameof(ResetPasswordAsync)}: user with provided email not found: {email}.");
                throw new InvalidOperationException($"user with provided email: {email} not found.");
            }

            try
            {
                var result = await _userManager.ResetPasswordAsync(user, Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token)), password);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"{nameof(ResetPasswordAsync)} for {user.Email} completed successfully.");
                    return result;
                }

                var errorDescription = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError($"ResetPasswordAsync for {user.Email} completed unsuccessfully: {errorDescription}");
                throw new InvalidOperationException($"ResetPasswordAsync for {user.Email} completed unsuccessfully: {errorDescription}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception occurred during password reset for {user.Email}: {ex.Message}");
                throw;
            }
        }

    }
}