using CandidateDashboardApi.Models;
using CandidateDashboardApi.Models.AccountServiceModels;
using CandidateDashboardApi.Models.ResponseModels;
using CandidateDashboardApi.Models.ResponseModels.AccountServiceResponses;
using CandidateDashboardApi.Models.ResponseModels.EmployerServiceResponses;
using CandidateDashboardApi.Services;
using Microsoft.AspNetCore.Authorization;
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
                var result = await _accountService.ConfirmUserEmailAsync(email, token);
                return Ok(new ConfirmEmailResponse { Message = $"Email successfully confirmed. {result}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ConfirmEmailResponse { Message = $"Failed to confirm email. {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest model)
        {
            try
            {
                await _accountService.ForgotPasswordAsync(model.Email);
                return Ok(new ForgotPasswordResponse { Message = "Check your email, reset link was sent." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPasswordAsync(string email, string token, [FromBody] ResetPaswordRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data.");
            }

            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest("Password and confirmation are not the same.");
            }

            var result = await _accountService.ResetPasswordAsync(email, token, model.Password);

            return result.Succeeded
                ? Ok(result)
                : BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}