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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Candidate>(builder =>
            {
                builder.HasOne(c => c.ApplicationUser)
                       .WithOne(u => u.Candidate)
                       .HasForeignKey<ApplicationUser>(u => u.CandidateId);
            });

            modelBuilder.Entity<Employer>(builder =>
            {
                builder.HasOne(e => e.ApplicationUser)
                       .WithOne(u => u.Employer)
                       .HasForeignKey<ApplicationUser>(u => u.EmployerId);
            });
        }
    }
}