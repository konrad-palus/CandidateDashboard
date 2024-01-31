using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Employer : ApplicationUser
    {
        [Required]
        public string CompanyName { get; set; }
        public string? CompanyLogo { get; set; }
        public string? CompanyDescription { get; set; }
        public ImportantSites ImportantSites { get; set; }
    }
}