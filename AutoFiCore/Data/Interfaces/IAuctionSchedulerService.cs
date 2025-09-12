using AutoFiCore.Dto;
using AutoFiCore.Utilities;

namespace AutoFiCore.Data.Interfaces
{
    public interface IAuctionSchedulerService
    {
        Task<Result<CreateAuctionDTO>> UpdateScheduledAuctionAsync(int auctionId, CreateAuctionDTO dto);
    }
}
