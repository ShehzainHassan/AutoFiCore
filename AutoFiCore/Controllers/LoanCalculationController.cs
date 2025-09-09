using AutoFiCore.Data;
using AutoFiCore.Models;
using AutoFiCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoFiCore.Controllers;

/// <summary>
/// Provides endpoints for calculating loan details based on user input.
/// </summary>
[ApiController]
[Route("[controller]")]
public class LoanCalculationController : ControllerBase
{
    private readonly ILoanService _loanService;
    private readonly ILogger<LoanCalculationController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoanCalculationController"/> class.
    /// </summary>
    /// <param name="loanService">Service for performing loan calculations.</param>
    /// <param name="logger">Logger instance for diagnostic logging.</param>
    public LoanCalculationController(ILoanService loanService, ILogger<LoanCalculationController> logger)
    {
        _loanService = loanService;
        _logger = logger;
    }

    /// <summary>
    /// Calculates loan repayment details based on the provided request parameters.
    /// </summary>
    /// <param name="request">Loan request containing VehicleId, LoanAmount, InterestRate, and LoanTermMonths.</param>
    /// <returns>
    /// Returns a <see cref="LoanCalculation"/> object containing calculated loan details,
    /// including monthly payment, total interest, total cost, and loan metadata such as vehicle ID,
    /// loan amount, interest rate, and term.
    /// </returns>
    [AllowAnonymous]
    [HttpPost("CalculateLoan")]
    [ProducesResponseType(typeof(LoanCalculation), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoanCalculation>> Calculate([FromBody] LoanRequest request)
    {
        var result = await _loanService.CalculateLoanAsync(request);
        return Ok(result);
    }
}