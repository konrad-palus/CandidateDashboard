using CandidateDashboardApi.Models.UserServiceModels;

namespace CandidateDashboardApi.Models.EmployerServiceModels
{
    public class EmployerDataModel
    {
        public string? CompanyName { get; set; }
        public string? CompanyDescription { get; set; }
        public ICollection<ImportantSitesDataModel> ImportantSites { get; set; }
    }
}
