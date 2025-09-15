using AutoFiCore.Models;
using AutoFiCore.Utilities;

namespace AutoFiCore.Data.Interfaces
{
    public interface ILoanService
    {
        Task<Result<LoanCalculation>> CalculateLoanAsync(LoanRequest request);
    }
}
