using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Employer
    {
        [Key]
        public string Id { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyDescription { get; set; }
        public ICollection<ImportantSites>? ImportantSites { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}