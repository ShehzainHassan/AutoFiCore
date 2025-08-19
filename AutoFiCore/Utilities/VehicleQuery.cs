using AutoFiCore.Data;
using AutoFiCore.Dto;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using Polly;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace AutoFiCore.Utilities
{
    /// <summary>
    /// Provides utility methods for querying and filtering <see cref="Vehicle"/> entities.
    /// </summary>

    public class VehicleQuery
    {
        /// <summary>
        /// Applies filtering criteria to the vehicle query based on provided parameters.
        /// </summary>
        /// <param name="query">The initial vehicle query.</param>
        /// <param name="make">Filter by vehicle make.</param>
        /// <param name="model">Filter by vehicle model.</param>
        /// <param name="startPrice">Minimum price filter.</param>
        /// <param name="endPrice">Maximum price filter.</param>
        /// <param name="mileage">Maximum mileage filter.</param>
        /// <param name="startYear">Minimum manufacturing year filter.</param>
        /// <param name="endYear">Maximum manufacturing year filter.</param>
        /// <param name="gearbox">Comma-separated list of gearbox types to filter.</param>
        /// <param name="selectedColors">Comma-separated list of colors to filter.</param>
        /// <param name="status">Filter by vehicle status.</param>
        /// <returns>The filtered <see cref="IQueryable{Vehicle}"/>.</returns>

        public static IQueryable<Vehicle> ApplyFilters(IQueryable<Vehicle> query, 
            string? make,
            string? model, 
            decimal? startPrice, 
            decimal? endPrice,
            int? mileage, 
            int? startYear, 
            int? endYear,
            string? gearbox,
            string? selectedColors,
            string? status
            )
        {
            if (!string.IsNullOrWhiteSpace(make))
                query = query.Where(v => v.Make == make);

            if (!string.IsNullOrWhiteSpace(model))
                query = query.Where(v => v.Model == model);

            if (startPrice.HasValue)
                query = query.Where(v => v.Price >= startPrice.Value);

            if (endPrice.HasValue)
                query = query.Where(v => v.Price <= endPrice.Value);

            if (mileage.HasValue)
                query = query.Where(v => v.Mileage <= mileage.Value);

            if (startYear.HasValue)
                query = query.Where(v => v.Year >= startYear.Value);

            if (endYear.HasValue)
                query = query.Where(v => v.Year <= endYear.Value);

            if (!string.IsNullOrWhiteSpace(gearbox))
            {
                var list = gearbox.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                query = query.Where(v => v.Transmission != null && list.Contains(v.Transmission));
            }

            if (!string.IsNullOrWhiteSpace(selectedColors))
            {
                var list = selectedColors.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                query = query.Where(v => v.Color != null && list.Contains(v.Color));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(v=>v.Status == status);
            }
            return query;
        }

        /// <summary>
        /// Applies sorting to the vehicle query based on the specified sort order.
        /// </summary>
        /// <param name="query">The vehicle query to sort.</param>
        /// <param name="sortOrder">The sort order string (e.g., "price_asc", "year_desc").</param>
        /// <returns>The sorted <see cref="IQueryable{Vehicle}"/>.</returns>

        public static IQueryable<Vehicle> ApplySorting(IQueryable<Vehicle> query, string? sortOrder)
        {
            return sortOrder switch
            {
                "price_asc" => query.OrderBy(v => v.Price),
                "price_desc" => query.OrderByDescending(v => v.Price),
                "mileage_asc" => query.OrderBy(v => v.Mileage),
                "mileage_desc" => query.OrderByDescending(v => v.Mileage),
                "year_asc" => query.OrderBy(v => v.Year),
                "year_desc" => query.OrderByDescending(v => v.Year),
                "name_asc" => query.OrderBy(v => v.Make).ThenBy(v => v.Model),
                "name_desc" => query.OrderByDescending(v => v.Make).ThenByDescending(v => v.Model),
                _ => query.OrderBy(v => v.Id),
            };
        }

        /// <summary>
        /// Retrieves a paginated list of vehicles from the query.
        /// </summary>
        /// <param name="query">The vehicle query.</param>
        /// <param name="offset">The number of records to skip.</param>
        /// <param name="pageView">The number of records to take.</param>
        /// <returns>A task that resolves to a list of vehicles.</returns>

        public static Task<List<Vehicle>> GetPaginatedVehiclesAsync(IQueryable<Vehicle> query, int offset, int pageView)
        {
            return query.Skip(offset).Take(pageView).ToListAsync();
        }

        /// <summary>
        /// Retrieves a dictionary of gearbox types and their respective counts.
        /// </summary>
        /// <param name="query">The vehicle query.</param>
        /// <returns>A task that resolves to a dictionary of gearbox counts.</returns>
        public static async Task<Dictionary<string, int>> GetGearboxCountsAsync(IQueryable<Vehicle> query)
        {
            var gearboxCounts = await query
                .Where(v => !string.IsNullOrEmpty(v.Transmission))
                .Select(v => v.Transmission!)
                .GroupBy(t => t)
                .Select(g => new { Transmission = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Transmission , g => g.Count);

            return gearboxCounts;
        }

        /// <summary>
        /// Retrieves a dictionary of selected colors and their respective counts.
        /// </summary>
        /// <param name="query">The vehicle query.</param>
        /// <returns>A task that resolves to a dictionary of color counts.</returns>
        public static async Task<Dictionary<string, int>> GetSelectedColorCounts(IQueryable<Vehicle> query)
        {
            var selectedColorsCounts = await query
                 .Where(v => !string.IsNullOrEmpty(v.Color))
                 .GroupBy(v => v.Color!)
                 .Select(c => new { Color = c.Key, Count = c.Count() })
                 .ToDictionaryAsync(c => c.Color, c => c.Count);
            return selectedColorsCounts;
        }
    }
}
