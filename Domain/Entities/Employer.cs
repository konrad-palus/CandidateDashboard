namespace Domain.Entities
{
    public class Employer : ApplicationUser
    {
        public string CompanyName { get; set; }
        public string CompanyDescription { get; set; }
        public ICollection<ImportantSites> ImportantSites { get; set; } = new HashSet<ImportantSites>();
    }
}