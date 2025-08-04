namespace AutoFiCore.Dto
{
    public class AuctionCompletionDTO
    {
        public int AuctionId { get; set; }
        public bool IsSuccessful { get; set; }
        public decimal FinalPrice { get; set; }

    }
}
