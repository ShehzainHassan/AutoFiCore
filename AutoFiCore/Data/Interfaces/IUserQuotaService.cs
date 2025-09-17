using AutoFiCore.Utilities;

namespace AutoFiCore.Data.Interfaces
{
    public interface IUserQuotaService
    {
        Task<Result<bool>> TryConsumeAsync(int userId);
        Task<Result<int>> GetRemainingAsync(int userId);
    }
}
