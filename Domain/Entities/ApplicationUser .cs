using Domain.Entities.CandidateEntities;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? LastName { get; set; }
        public string? ContactEmail { get; set; }
        public int? PhoneNumber { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? PhotoUrl { get; set; }
        public Candidate Candidate { get; set; }
        public Employer Employer { get; set; }
    }
}