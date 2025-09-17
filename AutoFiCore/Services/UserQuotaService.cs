using AutoFiCore.Data.Interfaces;
using AutoFiCore.Utilities;
using StackExchange.Redis;

namespace AutoFiCore.Services
{
    public class UserQuotaService : IUserQuotaService
    {
        private readonly IDatabase _db;
        private readonly int _dailyLimit = 5;

        public UserQuotaService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task<Result<bool>> TryConsumeAsync(int userId)
        {
            try
            {
                var today = DateTime.UtcNow.ToString("yyyyMMdd");
                var key = $"ai_quota:{userId}:{today}";

                var count = await _db.StringIncrementAsync(key);

                if (count == 1)
                {
                    var tomorrow = DateTime.UtcNow.Date.AddDays(1);
                    var expiry = tomorrow - DateTime.UtcNow;
                    await _db.KeyExpireAsync(key, expiry);
                }

                var allowed = count <= _dailyLimit;
                return allowed
                    ? Result<bool>.Success(true)
                    : Result<bool>.Failure("Daily quota exceeded.");
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure($"Quota check failed: {ex.Message}");
            }
        }

        public async Task<Result<int>> GetRemainingAsync(int userId)
        {
            try
            {
                var today = DateTime.UtcNow.ToString("yyyyMMdd");
                var key = $"ai_quota:{userId}:{today}";

                var count = (int)(await _db.StringGetAsync(key));
                var remaining = Math.Max(0, _dailyLimit - count);

                return Result<int>.Success(remaining);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure($"Failed to fetch remaining quota: {ex.Message}");
            }
        }
    }
}
