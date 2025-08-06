namespace AutoFiCore.Dto
{
    public class UserAnalyticsTableDTO
    {
        public string UserName { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public DateTime? LastActive { get; set; }
        public int TotalBids { get; set; }
        public int TotalWins { get; set; }
    }
}
