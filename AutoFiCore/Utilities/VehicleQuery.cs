using AutoFiCore.Data;
using AutoFiCore.Dto;
using AutoFiCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using Polly;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace AutoFiCore.Utilities
{
    public class VehicleQuery
    {

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

        public static Task<List<Vehicle>> GetPaginatedVehiclesAsync(IQueryable<Vehicle> query, int offset, int pageView)
        {
            return query.Skip(offset).Take(pageView).ToListAsync();
        }

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
