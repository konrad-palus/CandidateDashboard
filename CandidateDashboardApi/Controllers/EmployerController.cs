using CandidateDashboardApi.Interfaces;
using CandidateDashboardApi.Models;
using CandidateDashboardApi.Models.EmployerServiceModels;
using CandidateDashboardApi.Models.ResponseModels.EmployerServiceResponses;
using CandidateDashboardApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CandidateDashboardApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmployerController : ControllerBase
    {
        private readonly EmployerService _employerService;

        public EmployerController(
            EmployerService employerService)
        {
            _employerService = employerService;
        }

        [Authorize]
        [HttpPost("UpdateCompanyName")]
        public async Task<IActionResult> UpdateCompanyName([FromBody] UpdateCompanyNameModel model, string email)
        {
            try
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email); 
                var result = await _employerService.UpdateOrCreateCompanyName(email, model.CompanyName);
                return Ok(new UpdateCompanyNameResponseModel { CompanyName = result, Message = "Company name updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponseModel { Message = $"Company name updated unsuccessfully, {ex}" });
            }
        }

        [Authorize]
        [HttpPost("UpdateCompanyDescription")]
        public async Task<IActionResult> UpdateCompanyDescription([FromBody] UpdateCompanyDescriptionModel model)
        {
            try
            {
                var userEmail = User.FindFirstValue(ClaimTypes.Email); 
                var result = await _employerService.UpdateOrCreateCompanyDescription(userEmail, model.CompanyDescription);
                return Ok(new UpdateCompanyDescriptionResponseModel { CompanyDescription = result, Message = "Company description updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResponseModel { Message = $"Company name updated unsuccessfully, {ex}" });
            }
        }
    }
}
