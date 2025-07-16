using System.Text.Json.Serialization;
using AutoFiCore.Enums;

namespace AutoFiCore.Dto
{
    public class UpdateAuctionStatusDTO
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AuctionStatus Status { get; set; } = AuctionStatus.Active;

    }
}

