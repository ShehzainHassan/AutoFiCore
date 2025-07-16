using AutoFiCore.Dto;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoFiCore.Queries;

public static class AuctionQuery
{
    public static IQueryable<Auction> ApplyFilters(IQueryable<Auction> source, AuctionQueryParams filters)
    {
        if (filters.Status.HasValue)
            source = source.Where(a => a.Status == filters.Status.Value);

        if (!string.IsNullOrWhiteSpace(filters.Make))
            source = source.Where(a => a.Vehicle.Make == filters.Make);

        if (filters.MinPrice.HasValue)
            source = source.Where(a => a.CurrentPrice >= filters.MinPrice.Value);

        if (filters.MaxPrice.HasValue)
            source = source.Where(a => a.CurrentPrice <= filters.MaxPrice.Value);

        return source;
    }

    public static IQueryable<Auction> ApplySorting(IQueryable<Auction> source, AuctionQueryParams filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.SortBy))
        {
            return filters.SortBy.ToLower() switch
            {
                "price" => filters.Descending
                                    ? source.OrderByDescending(a => a.CurrentPrice)
                                    : source.OrderBy(a => a.CurrentPrice),

                "startutc" => filters.Descending
                                    ? source.OrderByDescending(a => a.StartUtc)
                                    : source.OrderBy(a => a.StartUtc),

                _ => source.OrderBy(a => a.AuctionId)
            };
        }

        return source.OrderBy(a => a.AuctionId);
    }
}
