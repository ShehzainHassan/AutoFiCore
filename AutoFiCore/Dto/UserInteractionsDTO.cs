namespace AutoFiCore.Dto
{
    public class UserInteractionsDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int VehicleId { get; set; }
        public string InteractionType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}