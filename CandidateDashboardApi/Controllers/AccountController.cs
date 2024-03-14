using CandidateDashboardApi.Models;
using CandidateDashboardApi.Models.AccountServiceModels;
using CandidateDashboardApi.Models.ResponseModels.EmployerServiceResponses;
using CandidateDashboardApi.Services;
using Microsoft.AspNetCore.Identity.Data;
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
                var response = await _accountService.RegisterUserAsync(model);
                return response.Success ? Ok(response) : BadRequest(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(new List<string> { ex.Message }, "Unexpected error occurred during registration."));
            }
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LogInModel model)
        {
            try
            {
                var response = await _accountService.LoginUserAsync(model.Login, model.Password);
                return response.Success ? Ok(response) : BadRequest(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(new List<string> { ex.Message }, "Unexpected error occurred during login."));
            }
        }

        [HttpGet]
        [Route("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string email, string token)
        {
            try
            {
                var response = await _accountService.ConfirmUserEmailAsync(email, token);
                return response.Success ? Ok(response) : BadRequest(response);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(new List<string> { ex.Message }, "Unexpected error occurred during confirming email."));
            }
        }

        [HttpPost]
        [Route("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest model)
        {
            try
            {
                var response = await _accountService.ForgotPasswordAsync(model.Email);
                return response.Success ? Ok(response) : BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>(new List<string> { ex.Message }, $"Unexpected error occurred during forgotPasword"));
            }
        }

        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPasswordAsync(string email, string token, [FromBody] ResetPaswordRequestModel model)
        {
            try
            {
                var response = await _accountService.ResetPasswordAsync(email, token, model.Password);
                return response.Success ? Ok(response) : BadRequest(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<bool>(new List<string> { ex.Message }, $"An unexpected error occurred during reseting password"));
            }
        }
    }
}