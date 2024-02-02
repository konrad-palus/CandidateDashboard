using Domain.Entities;
using Domain.Entities.CandidateEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Presistance;

namespace CandidateDashboardApi.Controllers.AccountController
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly CandidateDashboardContext _candidateDashboardContext;

        public AccountController
            (
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> singInManager,
            CandidateDashboardContext context
            )
        {
            _userManager = userManager;
            _signInManager = singInManager;
            _candidateDashboardContext = context;
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Registration(string login, string registrationEmail, string password, bool isCandidate, string? name, string? lastName)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser
            {
                UserName = login,
                Email = registrationEmail,
                Name = name,
                LastName = lastName,
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            if (isCandidate)
            {
                _candidateDashboardContext.Candidates.Add(new Candidate { Id = user.Id });

            }
            else
            {
                _candidateDashboardContext.Employers.Add(new Employer { Id = user.Id });
            }

            _candidateDashboardContext.SaveChanges();

            return Ok(new { UserId = user.Id });
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(string login, string password)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _signInManager.PasswordSignInAsync(login, password, isPersistent: true, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return Ok(new { IsSuccess = true });
            }
            else
            {
                return BadRequest(new { IsSuccess = false, Message = "Login unsuccessful" });
            }
        }
    }
}