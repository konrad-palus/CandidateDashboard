using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.CandidateEntities
{
    public class CandidateJobWanted
    {
        [Key]
        public string Id { get; set; }
        [Required]
        public string PositionName { get; set; }
        [Required]
        public List<PositionLevel> ExpectedPositionLevelList { get; set; }
        [Required]
        public List<JobType> JobTypeList { get; set; }
        [Required]
        public List<ContractType> ContractTypeList { get; set; }
        [Required]
        public int ExpectedWage { get; set; }
        public int? MinimumWage { get; set; }
        public virtual Candidate Candidate { get; set; }
    }
}