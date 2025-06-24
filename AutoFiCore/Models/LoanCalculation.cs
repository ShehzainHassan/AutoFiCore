namespace AutoFiCore.Models;

public class LoanCalculation
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public decimal LoanAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int LoanTermMonths { get; set; }
    public decimal MonthlyPayment { get; set; }
    public decimal TotalInterest { get; set; }
    public decimal TotalCost { get; set; }
} 