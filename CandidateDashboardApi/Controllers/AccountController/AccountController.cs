﻿using CandidateDashboardApi.Services;
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
        public async Task<IActionResult> Registration(string login, string registrationEmail, string password, bool isCandidate, string? name, string? lastName)
        {
            try
            {
                var userId = await _accountService.RegisterUserAsync(login, registrationEmail, password, isCandidate, name, lastName);
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
                var isSuccess = await _accountService.LoginUserAsync(login, password);
                return Ok(new { IsSuccess = isSuccess });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}