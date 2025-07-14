using AutoFiCore.Dto;
using AutoFiCore.Models;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AutoFiCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuctionController:ControllerBase
    {
        private readonly IAuctionService _auctionService;
        
        public AuctionController(IAuctionService auctionService)
        {
             _auctionService = auctionService;
        }

        //[Authorize(Roles = "Admin")]
        [HttpPost("create-auction")]
        public async Task<IActionResult> CreateAuction([FromBody] CreateAuctionDTO dto)
        {
            var result = await _auctionService.CreateAuctionAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            var auction = result.Value!;
            var response = new AuctionResponseDTO
            {
                AuctionId = auction.AuctionId,
                VehicleId = auction.VehicleId,
                StartUtc = auction.StartUtc,
                EndUtc = auction.EndUtc,
                StartingPrice = auction.StartingPrice,
                CurrentPrice = auction.CurrentPrice,
                Status = auction.Status
            };
            return Ok(response);
        }

        [HttpPut("/auction/{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateAuctionStatusDTO dto)
        {
            var result = await _auctionService.UpdateAuctionStatusAsync(id, dto.Status);

            if (!result.IsSuccess)
                return BadRequest(new { error = result.Error });

            return Ok(result.Value);
        }
    }
}
