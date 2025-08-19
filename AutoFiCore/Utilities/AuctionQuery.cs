using AutoFiCore.Dto;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoFiCore.Queries;

/// <summary>
/// Provides query extensions for filtering and sorting <see cref="Auction"/> entities.
/// </summary>
public static class AuctionQuery
{
    /// <summary>
    /// Applies filtering criteria to an <see cref="IQueryable{Auction}"/> based on the provided <see cref="AuctionQueryParams"/>.
    /// </summary>
    /// <param name="source">The source queryable of auctions.</param>
    /// <param name="filters">The filtering parameters.</param>
    /// <returns>A filtered <see cref="IQueryable{Auction}"/>.</returns>
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

    /// <summary>
    /// Applies sorting to an <see cref="IQueryable{Auction}"/> based on the provided <see cref="AuctionQueryParams"/>.
    /// </summary>
    /// <param name="source">The source queryable of auctions.</param>
    /// <param name="filters">The sorting parameters.</param>
    /// <returns>A sorted <see cref="IQueryable{Auction}"/>.</returns>
    public static IQueryable<Auction> ApplySorting(IQueryable<Auction> source, AuctionQueryParams filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.SortBy))
        {
            switch (filters.SortBy.ToLower())
            {
                case "price":
                    return filters.Descending
                        ? source.OrderByDescending(a => a.CurrentPrice)
                        : source.OrderBy(a => a.CurrentPrice);

                case "endtime":
                    return filters.Descending
                        ? source.OrderByDescending(a => a.EndUtc)
                        : source.OrderBy(a => a.EndUtc);

                case "make":
                    return filters.Descending
                        ? source.OrderByDescending(a => a.Vehicle.Make)
                        : source.OrderBy(a => a.Vehicle.Make);
            }
        }

        return source.OrderBy(a => a.AuctionId);
    }
}