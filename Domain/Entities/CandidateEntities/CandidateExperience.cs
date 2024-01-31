using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.CandidateEntities
{
    public class CandidateExperience
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [Required]
        public string CompanyName { get; set; }
        [Required]
        public string Role { get; set; }
        public string? Description { get; set; }
        [Required]
        public int CandidateId { get; set; }
        public virtual Candidate Candidate { get; set; }
    }
}