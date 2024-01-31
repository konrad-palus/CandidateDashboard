using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.CandidateEntities
{
    public class CandidateEducation
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [Required]
        public int CandidateId { get; set; }
        public virtual Candidate Candidate { get; set; }
    }
}