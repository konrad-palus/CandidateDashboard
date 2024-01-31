using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.CandidateEntities
{
    public class CandidateSkills
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string SkillName { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public int CandidateId { get; set; }
        public virtual Candidate Candidate { get; set; }
    }
}