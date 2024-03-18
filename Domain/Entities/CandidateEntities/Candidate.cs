namespace Domain.Entities.CandidateEntities
{
    public class Candidate : ApplicationUser
    {
        public string About { get; set; }
        public  ICollection<CandidateEducation> CandidateEducations { get; set;} = new List<CandidateEducation>();
        public  ICollection<CandidateExperience> CandidateExperience { get; set; } = new List<CandidateExperience>();
        public  ICollection<CandidateSkills> CandidateSkills { get; set; } = new HashSet<CandidateSkills>();
        public  ICollection<ImportantSites> ImportantSites { get; set; } = new HashSet<ImportantSites>();
        public  ICollection<CandidateJobWanted> CandidateJobWanted { get; set; } = new HashSet<CandidateJobWanted>();
    }
}