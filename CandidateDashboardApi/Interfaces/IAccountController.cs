using Microsoft.AspNetCore.Mvc;

namespace CandidateDashboardApi.Interfaces
{
    public interface IAccountController
    {
        public Task<IActionResult> RegisterUserAsync(string login, string registrationEmail, string password, bool isCandidate, string? name, string? lastName);
        public Task<IActionResult> LoginUserAsync(string login, string password);

    }
}