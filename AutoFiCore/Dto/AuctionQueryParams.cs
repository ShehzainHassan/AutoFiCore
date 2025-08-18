using AutoFiCore.Enums;
using System.Text.Json.Serialization;

public class AuctionQueryParams
{
    /// <summary>Status of the auction (Active, Ended, Scheduled, PreviewMode, Cancelled).</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AuctionStatus? Status { get; set; }

    /// <summary>Filter auctions by vehicle make.</summary>
    public string? Make { get; set; }

    /// <summary>Filter auctions with a minimum price.</summary>
    public decimal? MinPrice { get; set; }

    /// <summary>Filter auctions with a maximum price.</summary>
    public decimal? MaxPrice { get; set; }

    /// <summary>Sort auctions by a specific field (e.g., price, endtime, make).</summary>
    public string? SortBy { get; set; }

    /// <summary>If true, results will be sorted in descending order.</summary>
    public bool Descending { get; set; } = false;
}
