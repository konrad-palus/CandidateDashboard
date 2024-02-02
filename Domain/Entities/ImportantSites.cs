using Domain.Entities.CandidateEntities;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class ImportantSites
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string SiteName { get; set; }
        [Required]
        public string SiteUrl { get; set; }
    }
}