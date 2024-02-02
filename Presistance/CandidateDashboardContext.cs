using Microsoft.EntityFrameworkCore;
using Domain.Entities.CandidateEntities;
using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Presistance
{
    public class CandidateDashboardContext(DbContextOptions<CandidateDashboardContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Candidate> Candidates { get; set; }
        public DbSet<CandidateEducation> CandidateEducations { get; set; }
        public DbSet<CandidateExperience> CandidateExperiences { get; set; }
        public DbSet<CandidateJobWanted> CandidateJobWanteds { get; set; }
        public DbSet<CandidateSkills> CandidateSkills { get; set; }
        public DbSet<Employer> Employers { get; set; }
        public DbSet<ImportantSites> ImportantSites { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    }
}