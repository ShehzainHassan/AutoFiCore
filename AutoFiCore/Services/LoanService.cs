using AutoFiCore.Data.Interfaces;
using AutoFiCore.Models;
using AutoFiCore.Utilities;

namespace AutoFiCore.Services
{
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

        public async Task<Result<LoanCalculation>> CalculateLoanAsync(LoanRequest request)
        {
            if (request.LoanAmount <= 0)
            {
                _logger.LogWarning("Invalid loan amount: {Amount}", request.LoanAmount);
                return Result<LoanCalculation>.Failure("Loan amount must be greater than zero.");
            }

            var vehicle = await _vehicleRepository.GetVehicleByIdAsync(request.VehicleId);
            if (vehicle == null)
            {
                _logger.LogWarning("Vehicle with ID {VehicleId} not found", request.VehicleId);
                return Result<LoanCalculation>.Failure($"Vehicle with ID {request.VehicleId} not found.");
            }

            decimal monthlyRate = InterestRate / 100 / 12;

            decimal monthlyPayment = request.LoanAmount *
                (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), LoanTermMonths)) /
                ((decimal)Math.Pow((double)(1 + monthlyRate), LoanTermMonths) - 1);

            decimal totalCost = monthlyPayment * LoanTermMonths;
            decimal totalInterest = totalCost - request.LoanAmount;

            var calculation = new LoanCalculation
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

            _logger.LogInformation("Loan calculation completed for vehicle {VehicleId}", request.VehicleId);
            return Result<LoanCalculation>.Success(calculation);
        }
    }
}