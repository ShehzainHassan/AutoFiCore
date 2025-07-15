namespace AutoFiCore.Dto
{
    public class AuctionQueryParams
    {
        public string? Status { get; set; }
        public string? Make { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortBy { get; set; }
        public bool Descending { get; set; } = false;
    }
}
