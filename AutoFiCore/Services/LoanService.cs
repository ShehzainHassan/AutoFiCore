using AutoFiCore.Data;
using AutoFiCore.Models;

namespace AutoFiCore.Services
{
    public interface ILoanService
    {
        Task<LoanCalculation> CalculateLoanAsync(LoanRequest request);
    }
    public class LoanService : ILoanService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ILogger<LoanService> _logger;

        private const decimal InterestRate = 7.5m;
        private const int LoanTermMonths = 60;

        public LoanService(IVehicleRepository vehicleRepository, ILogger<LoanService> logger)
        {
            _vehicleRepository = vehicleRepository;
            _logger = logger;
        }

        public async Task<LoanCalculation> CalculateLoanAsync(LoanRequest request)
        {
            if (request.LoanAmount <= 0)
                throw new ArgumentException("Loan amount must be greater than zero");

            var vehicle = await _vehicleRepository.GetVehicleByIdAsync(request.VehicleId);
            if (vehicle == null)
                throw new ArgumentException($"Vehicle with ID {request.VehicleId} not found");

            decimal monthlyRate = InterestRate / 100 / 12;

            decimal monthlyPayment = request.LoanAmount *
                (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), LoanTermMonths)) /
                ((decimal)Math.Pow((double)(1 + monthlyRate), LoanTermMonths) - 1);

            decimal totalCost = monthlyPayment * LoanTermMonths;
            decimal totalInterest = totalCost - request.LoanAmount;

            return new LoanCalculation
            {
                Id = new Random().Next(1, 1000), 
                VehicleId = request.VehicleId,
                LoanAmount = request.LoanAmount,
                InterestRate = InterestRate,
                LoanTermMonths = LoanTermMonths,
                MonthlyPayment = Math.Round(monthlyPayment, 2),
                TotalInterest = Math.Round(totalInterest, 2),
                TotalCost = Math.Round(totalCost, 2)
            };
        }
    }
}
