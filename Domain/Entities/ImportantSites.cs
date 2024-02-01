using Domain.Entities.CandidateEntities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class ImportantSites
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string SiteName {  get; set; }
        [Required]
        public string SiteUrl { get; set; }
        public string? CandidateId { get; set; }
        public virtual Candidate? Candidate { get; set; }
        public string? EmployerId { get; set; }
        public virtual Employer? Employer { get; set; }
    }
}