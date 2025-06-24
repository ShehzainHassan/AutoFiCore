using Microsoft.AspNetCore.Mvc;
using AutoFiCore.Data;
using AutoFiCore.Models;
using AutoFiCore.Services;

namespace AutoFiCore.Controllers;

[ApiController]
[Route("[controller]")]
public class LoanCalculationController : ControllerBase
{
    private readonly ILoanService _loanService;
    private readonly ILogger<LoanCalculationController> _logger;

    public LoanCalculationController(ILoanService loanService, ILogger<LoanCalculationController> logger)
    {
        _loanService = loanService;
        _logger = logger;
    }

    [HttpPost("CalculateLoan")]
    public async Task<ActionResult<LoanCalculation>> Calculate([FromBody] LoanRequest request)
    {
        var result = await _loanService.CalculateLoanAsync(request);
        return Ok(result);
    }
}