using AutoFiCore.Models;
using AutoFiCore.Utilities;

namespace AutoFiCore.Data.Interfaces
{
    public interface INewsLetterService
    {
        Task<Result<Newsletter>> SubscribeToNewsLetterAsync(Newsletter newsletter);
    }
}
