using AutoFiCore.Enums;

namespace AutoFiCore.Dto
{
    public class UpdateAutoBidDTO
    {
        public decimal? MaxBidAmount { get; set; }
        public int? BidStrategyType { get; set; }
        public bool? IsActive { get; set; }
    }
}
