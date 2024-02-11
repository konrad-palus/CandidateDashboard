using CandidateDashboardApi.Services;
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
                var userId = await _accountService.RegisterUserAsync(model.Login, model.RegistrationEmail, model.Password, model.IsCandidate, model.Name, model.LastName);
                return Ok(new { UserId = userId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(string login, string password)
        {
            try
            {
                var token = await _accountService.LoginUserAsync(login, password);
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}