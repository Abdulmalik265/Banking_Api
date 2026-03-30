using BankingApi.Data;
using BankingApi.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BankingApi.Services;

public class AccountService
{
    private readonly BankingDbContext _context;

    public AccountService(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<AccountInfoResponse>> GetAccountInfo(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Account)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return new ApiResponse<AccountInfoResponse>(404, "User not found");

        if (user.Account == null)
            return new ApiResponse<AccountInfoResponse>(404, "Account not found");

        return new ApiResponse<AccountInfoResponse>(200, "Success", new AccountInfoResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            AccountNumber = user.Account.AccountNumber,
            Balance = user.Account.Balance,
            Currency = user.Account.Currency,
            Status = user.Account.Status,
            CreatedAt = user.Account.CreatedAt
        });
    }

    public async Task<ApiResponse<AccountInfoResponse>> UpdateAccount(int userId, UpdateAccountRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Account)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return new ApiResponse<AccountInfoResponse>(404, "User not found");

        if (!string.IsNullOrWhiteSpace(request.FullName))
            user.FullName = request.FullName.Trim();

        if (!string.IsNullOrWhiteSpace(request.Phone))
            user.Phone = request.Phone.Trim();

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new ApiResponse<AccountInfoResponse>(200, "Account updated successfully", new AccountInfoResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            AccountNumber = user.Account.AccountNumber,
            Balance = user.Account.Balance,
            Currency = user.Account.Currency,
            Status = user.Account.Status,
            CreatedAt = user.Account.CreatedAt
        });
    }
}
