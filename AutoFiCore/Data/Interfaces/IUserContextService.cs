using AutoFiCore.Dto;
using AutoFiCore.Utilities;

namespace AutoFiCore.Data.Interfaces
{
    public interface IUserContextService
    {
        Task<Result<UserContextDTO>> GetUserContextAsync(int userId);
        Task<Result<MLUserContextDto?>> FetchMLContextAsync(int userId, string correlationId, string jwtToken);
    }
}
