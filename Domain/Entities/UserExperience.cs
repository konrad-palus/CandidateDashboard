namespace Domain.Entities
{
    public class UserExperience
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CompanyName { get; set; }
        public string Role { get; set; }
        public string Description { get; set; }
    }
}