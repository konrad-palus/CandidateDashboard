using CandidateDashboardApi.Models;
using CandidateDashboardApi.Models.ResponseModels;
using CandidateDashboardApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CandidateDashboardApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly AccountService _accountService;

        public AccountController(AccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Registration([FromBody] RegistrationModel model)
        {
            try
            {
                var userId = await _accountService.RegisterUserAsync(model);
                return Ok(new RegistrationResponse { UserId = userId, Message = "Registration successful." });
            }
            catch (Exception ex)
            {
                return BadRequest(new RegistrationResponse { Message = $"Registration failed. {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] SignInModel model)
        {
            try
            {
                var token = await _accountService.LoginUserAsync(model.Login, model.Password);
                return Ok(new LoginResponse { Token = token, Message = "Login successful." });
            }
            catch (Exception ex)
            {
                return BadRequest(new LoginResponse { Message = $"Login failed. {ex.Message}" });
            }
        }

        [Authorize]
        [HttpGet("GetUserData")]
        public async Task<IActionResult> GetUserData()
        {
            try
            {
                var userData = await _accountService.GetUserDataAsync(User);
                return Ok(new GetUserDataResponse { UserData = userData, Message = "User data retrieval successful." });
            }
            catch (Exception ex)
            {
                return BadRequest(new GetUserDataResponse { Message = $"Failed to retrieve user data. {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string email, string token)
        {
            try
            {
                var result = await _accountService.ConfirmUserEmailAsync(email, token);
                return Ok(new ConfirmEmailResponse { Message = $"Email successfully confirmed. {result}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ConfirmEmailResponse { Message = $"Failed to confirm email. {ex.Message}" });
            }
        }
    }
}