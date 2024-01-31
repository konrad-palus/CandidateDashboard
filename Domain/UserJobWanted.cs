namespace Domain
{
    public class UserJobWanted
    {
        public int Id { get; set; }
        public string PositionName { get; set; }
        public List<PositionLevel> ExpectedPositionLevelList { get; set; }
        public List<JobType> JobTypeList { get; set; }
        public List<ContractType> ContractTypeList { get; set; }
        public int MinimumWage { get; set; }
        public int ExpectedWage { get; set; }
    }
}