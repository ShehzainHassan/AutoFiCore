using AutoFiCore.Data;
using AutoFiCore.Dto;
using Microsoft.Extensions.Caching.Memory;

public class CachedVehicleRepository
{
    private readonly IVehicleRepository _repository;
    private readonly IMemoryCache _cache;

    public CachedVehicleRepository(IVehicleRepository repository, IMemoryCache cache)
    {
        _repository = repository;
        _cache = cache;
    }
    public async Task<List<string>> GetDistinctColorsAsync()
    {
        const string cacheKey = "vehicle-colors";

        if (_cache.TryGetValue(cacheKey, out List<string>? cachedColors))
            return cachedColors!;

        var colors = await _repository.GetDistinctColorsAsync();

        _cache.Set(cacheKey, colors, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
            SlidingExpiration = TimeSpan.FromMinutes(15)
        });

        return colors;
    }
    public async Task<List<VehicleOptionsDTO>> GetVehicleOptionsAsync()
    {
        const string cacheKey = "vehicle-options";

        if (_cache.TryGetValue(cacheKey, out List<VehicleOptionsDTO>? cachedOptions))
            return cachedOptions!;

        var options = await _repository.GetVehicleOptionsAsync();

        _cache.Set(cacheKey, options, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
            SlidingExpiration = TimeSpan.FromMinutes(15)
        });

        return options;
    }

}
