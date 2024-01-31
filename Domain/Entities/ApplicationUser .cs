namespace Domain.Entities
{
    public class ApplicationUser
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? ContactEmail { get; set; }
        public int? PhoneNumber { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? PhotoUrl { get; set; }
        public string? About { get; set; }
    }
}