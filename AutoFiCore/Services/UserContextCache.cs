using AutoFiCore.Dto;
using Microsoft.Extensions.Caching.Memory;

public interface IUserContextCache
{
    Task<UserContextDTO> GetOrAddAsync(int userId, Func<Task<UserContextDTO>> factory);
}

public class UserContextCache : IUserContextCache
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private readonly TimeSpan _expiration = TimeSpan.FromMinutes(5);

    public async Task<UserContextDTO> GetOrAddAsync(int userId, Func<Task<UserContextDTO>> factory)
    {
        if (_cache.TryGetValue(userId, out UserContextDTO cached) && cached != null)
        {
            Console.WriteLine($"Cache hit for user {userId}");
            return cached;
        }

        Console.WriteLine($"Cache miss for user {userId}");
        var value = await factory();

        if (value != null)
            _cache.Set(userId, value, _expiration);

        return value;
    }
}
