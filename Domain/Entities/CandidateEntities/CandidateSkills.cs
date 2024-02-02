using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.CandidateEntities
{
    public class CandidateSkills
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string SkillName { get; set; }
        [Required]
        public string? Description { get; set; }
        public virtual Candidate Candidate { get; set; }
    }
}