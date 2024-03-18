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
        private readonly string _baseUrl;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            CandidateDashboardContext context,
            IConfiguration configuration,
            IEmailService emailService,
            IUrlHelperFactory factory,
            IActionContextAccessor actionContextAccessor,
            ILogger<AccountService> logger,
            IMapper mapper,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _candidateDashboardContext = context;
            _configuration = configuration;
            _emailService = emailService;
            _urlHelper = factory.GetUrlHelper(actionContextAccessor.ActionContext!);
            _actionContextAccessor = actionContextAccessor;
            _logger = logger;
            _mapper = mapper;

            _baseUrl = _configuration["AppSettings:BaseUrl"]!;
            if (env.IsDevelopment())
            {
                _baseUrl = _configuration["AppSettings:DevelopmentBaseUrl"]!;
            }
        }

        public async Task<ApiResponse<string>> RegisterUserAsync(RegistrationModel model)
        {
            _logger.LogInformation("{MethodName} -> Starting registration process for email: {Email}", nameof(RegisterUserAsync), model.RegistrationEmail);

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
                _logger.LogError("{MethodName} -> User not found for email: {Email}", nameof(GenerateJwtTokenAsync), email);
                throw new InvalidOperationException("User not found.");
            }

            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                 new Claim(JwtRegisteredClaimNames.Sub, user.Email!),
                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                 new Claim(ClaimTypes.NameIdentifier, user.Id),

            }.Union(userClaims).ToList();

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<ApiResponse<string>> LoginUserAsync(string login, string password)
        {
            try
            {
                _logger.LogInformation("Login attempt for {Login}", login);

                var user = await _userManager.FindByNameAsync(login);
                if (user == null)
                {
                    _logger.LogWarning("{methodName} -> Login failed for {Login}: User not found", nameof(LoginUserAsync), login);

                    return new ApiResponse<string>(new List<string> { $"User not found." }, "User not found");
                }

                var result = await _signInManager.PasswordSignInAsync(user.UserName, password, isPersistent: true, lockoutOnFailure: false);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("{MethodName} ->  Login failed for {Login}", nameof(LoginUserAsync), login);

                    return new ApiResponse<string>(new List<string> { $"Invalid login attempt." }, "Invalid login attempt");
                }

                var token = await GenerateJwtTokenAsync(user.Email);

                return new ApiResponse<string>(token, "Login successful.");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError("{MethodName} -> Exception: {ex} during the login process for {Login}.", nameof(LoginUserAsync), ex, login);

                return new ApiResponse<string>(new List<string> { ex.Message }, "Unexpected exepction during login");
            }
        }

        public async Task<ApiResponse<bool>> ConfirmUserEmailAsync(string email, string token)
        {
            _logger.LogInformation("{MethodName} -> Attempting to confirm email for user: {Email}", nameof(ConfirmUserEmailAsync), email);
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            {
                return new ApiResponse<bool>(false, "Email or token is null or empty");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogError("{MethodName} -> Email confirmation failed: user with email does not exist {Email}", nameof(ConfirmUserEmailAsync), email);

                return new ApiResponse<bool>(false, "User with provided email does not exist");
            }

            var result = await _userManager.ConfirmEmailAsync(user, Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token)));
            if (result.Succeeded)
            {
                _logger.LogInformation("{MethodName} -> Email successfully confirmed for: {Email}", nameof(ConfirmUserEmailAsync), email);

                return new ApiResponse<bool>(true, "Email successfully confirmed.");
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("{MethodName} -> Email confirmation failed for: {Email}. Errors: {Errors}", nameof(ConfirmUserEmailAsync), email, errors);

            return new ApiResponse<bool>(false, $"Email confirmation failed. Errors: {errors}");
        }

        public async Task<ApiResponse<bool>> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                var token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(await _userManager.GeneratePasswordResetTokenAsync(user)));
                var callbackUrl = $"{_baseUrl}/welcome/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";
                _logger.LogInformation("{MethodName} -> User {Email} requested password reset", nameof(ForgotPasswordAsync), email);

                await _emailService.SendEmailAsync(user.Email, "Reset hasła", $"Aby zresetować hasło kliknij <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>tutaj</a>.");

                return new ApiResponse<bool>(true, "Reset link was sent.");
            }

            _logger.LogWarning("{MethodName} -> Forgot password request for user who does not exist: {Email}", nameof(ForgotPasswordAsync), email);

            return new ApiResponse<bool>(false, "User not found.");
        }

        public async Task<ApiResponse<bool>> ResetPasswordAsync(string email, string token, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogWarning("{MethodName} -> User with provided email not found: {Email}", nameof(ResetPasswordAsync), email);

                return new ApiResponse<bool>(false, $"User with provided email: {email} not found.");
            }

            var result = await _userManager.ResetPasswordAsync(user, Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token)), password);
            if (result.Succeeded)
            {
                _logger.LogInformation("{MethodName} -> Password reset successful for {Email}", nameof(ResetPasswordAsync), email);

                return new ApiResponse<bool>(true, "Password has been reset successfully.");
            }

            var errorDescription = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("{MethodName} -> Password reset for {Email} failed: {ErrorDescription}", nameof(ResetPasswordAsync), email, errorDescription);

            return new ApiResponse<bool>(false, $"Password reset failed: {errorDescription}");
        }
    }
}