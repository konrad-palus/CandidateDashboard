using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class ImportantSites
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string SiteName {  get; set; }
        [Required]
        public string SiteUrl { get; set; }
        [Required]
        public int ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}