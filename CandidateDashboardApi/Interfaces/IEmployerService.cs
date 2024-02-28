namespace CandidateDashboardApi.Interfaces
{
    public interface IEmployerService
    {
        Task<string> UpdateOrCreateCompanyName(string userEmail, string companyName);
        Task<string> UpdateOrCreateCompanyDescription(string userEmail, string companyDescription);
    }
}
