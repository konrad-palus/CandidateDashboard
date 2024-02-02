using Domain.Entities;
using Domain.Entities.CandidateEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Presistance;

namespace CandidateDashboardApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CandidatesController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly CandidateDashboardContext _candidateDashboardContext;

        public CandidatesController(UserManager<ApplicationUser> userManager, CandidateDashboardContext context)
        {
            _userManager = userManager;
            _candidateDashboardContext = context;
        }

        [HttpPost]
        public async Task<IActionResult> Registration(string login, string registrationEmail, string password, bool isCandidate, string name, string lastName)
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
    }
}