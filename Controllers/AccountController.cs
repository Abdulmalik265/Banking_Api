using BankingApi.DTOs;
using BankingApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountController : ControllerBase
{
    private readonly AccountService _accountService;

    public AccountController(AccountService accountService)
    {
        _accountService = accountService;
    }

    
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<AccountInfoResponse>), 200)]
    public async Task<IActionResult> GetAccountInfo()
    {
        var userId = GetUserId();
        var result = await _accountService.GetAccountInfo(userId);
        return StatusCode(result.Code, result);
    }

 
    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<AccountInfoResponse>), 200)]
    public async Task<IActionResult> UpdateAccount([FromBody] UpdateAccountRequest request)
    {
        var userId = GetUserId();
        var result = await _accountService.UpdateAccount(userId, request);
        return StatusCode(result.Code, result);
    }

    private int GetUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
