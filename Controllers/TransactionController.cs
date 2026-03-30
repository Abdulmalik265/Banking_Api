using BankingApi.DTOs;
using BankingApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionController : ControllerBase
{
    private readonly TransactionService _transactionService;

    public TransactionController(TransactionService transactionService)
    {
        _transactionService = transactionService;
    }

   
    [HttpPost("transfer")]
    [ProducesResponseType(typeof(ApiResponse<TransactionResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<TransactionResponse>), 400)]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
    {
        var userId = GetUserId();
        var result = await _transactionService.Transfer(userId, request);
        return StatusCode(result.Code, result);

    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(ApiResponse<List<TransactionResponse>>), 200)]
    public async Task<IActionResult> GetTransactionHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        var result = await _transactionService.GetTransactionHistory(userId, page, pageSize);
        return StatusCode(result.Code, result);
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
