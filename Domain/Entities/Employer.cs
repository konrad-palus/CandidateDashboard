namespace Domain.Entities
{
    public class Employer : ApplicationUser
    {
        public string CompanyName { get; set; }
        public string? CompanyLogo { get; set; }
        public string? CompanyDescription { get; set; }
        public string? CompanySiteLink { get; set; }
    }
}