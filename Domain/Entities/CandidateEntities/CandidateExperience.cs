using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.CandidateEntities
{
    public class CandidateExperience
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [Required]
        public string CompanyName { get; set; }
        [Required]
        public string Role { get; set; }
        public string Description { get; set; }
        public  Candidate Candidate { get; set; }
    }
}