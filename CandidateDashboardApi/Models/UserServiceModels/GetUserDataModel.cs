﻿namespace CandidateDashboardApi.Models.UserServiceModels
{
    public class GetUserDataModel
    {
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? ContactEmail { get; set; }
        public int? PhoneNumber { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
    }
}
