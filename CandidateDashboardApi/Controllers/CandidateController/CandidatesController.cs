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
        public async Task<IActionResult> Create(string login, string registrationEmail, string contactEmail, string password, string name, string surname)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser
            {
                UserName = login,
                Email = registrationEmail,
                ContactEmail = contactEmail,
                Name = name,
                Surname = surname
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

             var candidate = new Candidate { Id = user.Id };
             _candidateDashboardContext.Candidates.Add(candidate);
             await _candidateDashboardContext.SaveChangesAsync();

            return Ok(new { UserId = user.Id });
        }
    }
}