namespace Domain.Entities.CandidateEntities
{
    public class Candidate : ApplicationUser
    {
        public string? About { get; set; }
        public virtual ICollection<CandidateEducation> CandidateEducations { get; set;}
        public virtual ICollection<CandidateExperience> CandidateExperience { get; set; }
        public virtual ICollection<CandidateSkills> CandidateSkills { get; set; }
        public virtual ICollection<ImportantSites> ImportantSites { get; set; }
        public virtual ICollection<CandidateJobWanted> CandidateJobWanted { get; set; }

    }
}