namespace AutoFiCore.Dto
{
    public class CreateBidDTO
    {
        public decimal Amount { get; set; }
        public int UserId { get; set; }
        public bool? IsAuto { get; set; }
    }
}
