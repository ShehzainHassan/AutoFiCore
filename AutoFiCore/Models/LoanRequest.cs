namespace AutoFiCore.Models;

public class LoanRequest
{
    public int VehicleId { get; set; }
    public decimal LoanAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int LoanTermMonths { get; set; }
} 