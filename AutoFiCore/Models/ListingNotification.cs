namespace AutoFiCore.Models
{
    public class ListingNotification
    {
        public int VehicleId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
