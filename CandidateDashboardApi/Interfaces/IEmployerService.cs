namespace CandidateDashboardApi.Interfaces
{
    public interface IEmployerService
    {
        Task<string> UpdateOrCreateCompanyNameAsync(string userEmail, string companyName);
        Task<string> UpdateOrCreateCompanyDescriptionAsync(string userEmail, string companyDescription);
        Task<string> GetCompanyNameAsync(string userEmail);
        Task<string> GetCompanyDescriptionAsync(string userEmail);
    }
}
