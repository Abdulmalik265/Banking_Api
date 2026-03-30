using BankingApi.Data;
using BankingApi.DTOs;
using BankingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BankingApi.Services;

public class TransactionService
{
    private readonly BankingDbContext _context;

    public TransactionService(BankingDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<TransactionResponse>> Transfer(int senderUserId, TransferRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var senderAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.UserId == senderUserId);

            if (senderAccount == null)
                return new ApiResponse<TransactionResponse>(404, "Sender account not found");

            if (senderAccount.Status != "Active")
                return new ApiResponse<TransactionResponse>(400, "Your account is not active");

            var recipientAccount = await _context.Accounts
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.AccountNumber == request.RecipientAccountNumber.Trim());

            if (recipientAccount == null)
                return new ApiResponse<TransactionResponse>(404, "Recipient account not found");

            if (recipientAccount.Status != "Active")
                return new ApiResponse<TransactionResponse>(400, "Recipient account is not active");

            if (senderAccount.Id == recipientAccount.Id)
                return new ApiResponse<TransactionResponse>(400, "Cannot transfer to your own account");

            if (senderAccount.Balance < request.Amount)
                return new ApiResponse<TransactionResponse>(400, "Insufficient balance");

            // Record balances before
            var senderBalanceBefore = senderAccount.Balance;
            var recipientBalanceBefore = recipientAccount.Balance;

            // Update balances
            senderAccount.Balance -= request.Amount;
            senderAccount.UpdatedAt = DateTime.UtcNow;

            recipientAccount.Balance += request.Amount;
            recipientAccount.UpdatedAt = DateTime.UtcNow;

            // Create transaction record
            var reference = $"TXN{DateTime.UtcNow:yyyyMMddHHmmssfff}{new Random().Next(1000, 9999)}";

            var txn = new Transaction
            {
                Reference = reference,
                SenderId = senderUserId,
                RecipientId = recipientAccount.UserId,
                Amount = request.Amount,
                SenderBalanceBefore = senderBalanceBefore,
                SenderBalanceAfter = senderAccount.Balance,
                RecipientBalanceBefore = recipientBalanceBefore,
                RecipientBalanceAfter = recipientAccount.Balance,
                Status = "Successful",
                Narration = request.Narration,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Transactions.AddAsync(txn);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var senderUser = await _context.Users.FindAsync(senderUserId);

            return new ApiResponse<TransactionResponse>(200, "Transfer successful", new TransactionResponse
            {
                Id = txn.Id,
                Reference = txn.Reference,
                SenderName = senderUser?.FullName ?? "",
                SenderAccount = senderAccount.AccountNumber,
                RecipientName = recipientAccount.User.FullName,
                RecipientAccount = recipientAccount.AccountNumber,
                Amount = txn.Amount,
                SenderBalanceBefore = txn.SenderBalanceBefore,
                SenderBalanceAfter = txn.SenderBalanceAfter,
                Status = txn.Status,
                Narration = txn.Narration,
                CreatedAt = txn.CreatedAt
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            return new ApiResponse<TransactionResponse>(500, "Transfer failed. Please try again.");
        }
    }

    public async Task<ApiResponse<List<TransactionResponse>>> GetTransactionHistory(int userId, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _context.Transactions
            .Where(t => t.SenderId == userId || t.RecipientId == userId)
            .Include(t => t.Sender).ThenInclude(u => u.Account)
            .Include(t => t.Recipient).ThenInclude(u => u.Account)
            .OrderByDescending(t => t.CreatedAt);

        var total = await query.CountAsync();

        var transactions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TransactionResponse
            {
                Id = t.Id,
                Reference = t.Reference,
                SenderName = t.Sender.FullName,
                SenderAccount = t.Sender.Account.AccountNumber,
                RecipientName = t.Recipient.FullName,
                RecipientAccount = t.Recipient.Account.AccountNumber,
                Amount = t.Amount,
                SenderBalanceBefore = t.SenderBalanceBefore,
                SenderBalanceAfter = t.SenderBalanceAfter,
                Status = t.Status,
                Narration = t.Narration,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        return new ApiResponse<List<TransactionResponse>>(200, $"Found {total} transactions", transactions);
    }
}
