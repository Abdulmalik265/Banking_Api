using BankingApi.DTOs;
using BankingApi.Models;
using BankingApi.Services;
using BankingApi.Tests.Helpers;

namespace BankingApi.Tests;

public class TransactionServiceTests
{
    private async Task<(int senderId, int recipientId)> SeedUsers(BankingApi.Data.BankingDbContext context)
    {
        var sender = new User { FullName = "Sender", Email = "sender@test.com", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        var recipient = new User { FullName = "Recipient", Email = "recipient@test.com", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        context.Users.AddRange(sender, recipient);
        await context.SaveChangesAsync();

        context.Accounts.AddRange(
            new Account { UserId = sender.Id, AccountNumber = "1111111111", Balance = 50000, Currency = "NGN", Status = "Active", CreatedAt = DateTime.UtcNow },
            new Account { UserId = recipient.Id, AccountNumber = "2222222222", Balance = 10000, Currency = "NGN", Status = "Active", CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        return (sender.Id, recipient.Id);
    }

    [Fact]
    public async Task Transfer_ValidAmount_Succeeds()
    {
        var context = TestDbHelper.CreateContext("Transfer_Valid");
        var (senderId, _) = await SeedUsers(context);
        var service = new TransactionService(context);

        var result = await service.Transfer(senderId, new TransferRequest
        {
            RecipientAccountNumber = "2222222222",
            Amount = 5000,
            Narration = "Test transfer"
        });

        Assert.Equal(200, result.Code);
        Assert.Equal(5000, result.Data!.Amount);
        Assert.Equal(50000, result.Data.SenderBalanceBefore);
        Assert.Equal(45000, result.Data.SenderBalanceAfter);
    }

    [Fact]
    public async Task Transfer_InsufficientBalance_Fails()
    {
        var context = TestDbHelper.CreateContext("Transfer_Insufficient");
        var (senderId, _) = await SeedUsers(context);
        var service = new TransactionService(context);

        var result = await service.Transfer(senderId, new TransferRequest
        {
            RecipientAccountNumber = "2222222222",
            Amount = 999999
        });

        Assert.Equal(400, result.Code);
        Assert.Contains("Insufficient", result.Message);
    }

    [Fact]
    public async Task Transfer_ToSelf_Fails()
    {
        var context = TestDbHelper.CreateContext("Transfer_Self");
        var (senderId, _) = await SeedUsers(context);
        var service = new TransactionService(context);

        var result = await service.Transfer(senderId, new TransferRequest
        {
            RecipientAccountNumber = "1111111111",
            Amount = 1000
        });

        Assert.Equal(400, result.Code);
        Assert.Contains("own account", result.Message);
    }

    [Fact]
    public async Task Transfer_InvalidRecipient_ReturnsNotFound()
    {
        var context = TestDbHelper.CreateContext("Transfer_InvalidRecipient");
        var (senderId, _) = await SeedUsers(context);
        var service = new TransactionService(context);

        var result = await service.Transfer(senderId, new TransferRequest
        {
            RecipientAccountNumber = "9999999999",
            Amount = 1000
        });

        Assert.Equal(404, result.Code);
    }

    [Fact]
    public async Task Transfer_UpdatesRecipientBalance()
    {
        var context = TestDbHelper.CreateContext("Transfer_RecipientBalance");
        var (senderId, recipientId) = await SeedUsers(context);
        var service = new TransactionService(context);

        await service.Transfer(senderId, new TransferRequest
        {
            RecipientAccountNumber = "2222222222",
            Amount = 3000
        });

        var recipientAccount = context.Accounts.First(a => a.UserId == recipientId);
        Assert.Equal(13000, recipientAccount.Balance);
    }

    [Fact]
    public async Task GetTransactionHistory_ReturnsTransactions()
    {
        var context = TestDbHelper.CreateContext("History_Valid");
        var (senderId, _) = await SeedUsers(context);
        var service = new TransactionService(context);

        await service.Transfer(senderId, new TransferRequest
        {
            RecipientAccountNumber = "2222222222",
            Amount = 1000,
            Narration = "First"
        });

        await service.Transfer(senderId, new TransferRequest
        {
            RecipientAccountNumber = "2222222222",
            Amount = 2000,
            Narration = "Second"
        });

        var result = await service.GetTransactionHistory(senderId, 1, 10);

        Assert.Equal(200, result.Code);
        Assert.Equal(2, result.Data!.Count);
    }

    [Fact]
    public async Task GetTransactionHistory_NoTransactions_ReturnsEmpty()
    {
        var context = TestDbHelper.CreateContext("History_Empty");
        var (senderId, _) = await SeedUsers(context);
        var service = new TransactionService(context);

        var result = await service.GetTransactionHistory(senderId, 1, 10);

        Assert.Equal(200, result.Code);
        Assert.Empty(result.Data!);
    }

    [Fact]
    public async Task Transfer_InactiveSender_Fails()
    {
        var context = TestDbHelper.CreateContext("Transfer_InactiveSender");
        var (senderId, _) = await SeedUsers(context);

        // Deactivate sender account
        var senderAccount = context.Accounts.First(a => a.UserId == senderId);
        senderAccount.Status = "Inactive";
        await context.SaveChangesAsync();

        var service = new TransactionService(context);

        var result = await service.Transfer(senderId, new TransferRequest
        {
            RecipientAccountNumber = "2222222222",
            Amount = 1000
        });

        Assert.Equal(400, result.Code);
        Assert.Contains("not active", result.Message);
    }

    [Fact]
    public async Task Transfer_InactiveRecipient_Fails()
    {
        var context = TestDbHelper.CreateContext("Transfer_InactiveRecipient");
        var (senderId, recipientId) = await SeedUsers(context);

        // Deactivate recipient account
        var recipientAccount = context.Accounts.First(a => a.UserId == recipientId);
        recipientAccount.Status = "Inactive";
        await context.SaveChangesAsync();

        var service = new TransactionService(context);

        var result = await service.Transfer(senderId, new TransferRequest
        {
            RecipientAccountNumber = "2222222222",
            Amount = 1000
        });

        Assert.Equal(400, result.Code);
        Assert.Contains("not active", result.Message);
    }

    [Fact]
    public async Task Transfer_ExactBalance_Succeeds()
    {
        var context = TestDbHelper.CreateContext("Transfer_ExactBalance");
        var (senderId, _) = await SeedUsers(context);
        var service = new TransactionService(context);

        var result = await service.Transfer(senderId, new TransferRequest
        {
            RecipientAccountNumber = "2222222222",
            Amount = 50000
        });

        Assert.Equal(200, result.Code);
        Assert.Equal(0, result.Data!.SenderBalanceAfter);
    }

    [Fact]
    public async Task Transfer_SenderAccountNotFound_Returns404()
    {
        var context = TestDbHelper.CreateContext("Transfer_NoSenderAccount");
        var service = new TransactionService(context);

        var result = await service.Transfer(999, new TransferRequest
        {
            RecipientAccountNumber = "2222222222",
            Amount = 1000
        });

        Assert.Equal(404, result.Code);
        Assert.Contains("Sender", result.Message);
    }

    [Fact]
    public async Task GetTransactionHistory_Pagination_ReturnsCorrectPage()
    {
        var context = TestDbHelper.CreateContext("History_Pagination");
        var (senderId, _) = await SeedUsers(context);
        var service = new TransactionService(context);

        // Create 3 transactions
        for (int i = 0; i < 3; i++)
        {
            await service.Transfer(senderId, new TransferRequest
            {
                RecipientAccountNumber = "2222222222",
                Amount = 1000,
                Narration = $"Transfer {i + 1}"
            });
        }

        var page1 = await service.GetTransactionHistory(senderId, 1, 2);
        var page2 = await service.GetTransactionHistory(senderId, 2, 2);

        Assert.Equal(2, page1.Data!.Count);
        Assert.Single(page2.Data!);
    }

    [Fact]
    public async Task GetTransactionHistory_PageBeyondRange_ReturnsEmpty()
    {
        var context = TestDbHelper.CreateContext("History_BeyondRange");
        var (senderId, _) = await SeedUsers(context);
        var service = new TransactionService(context);

        await service.Transfer(senderId, new TransferRequest
        {
            RecipientAccountNumber = "2222222222",
            Amount = 1000
        });

        var result = await service.GetTransactionHistory(senderId, 100, 10);

        Assert.Equal(200, result.Code);
        Assert.Empty(result.Data!);
    }

    [Fact]
    public async Task GetTransactionHistory_AsRecipient_ShowsTransaction()
    {
        var context = TestDbHelper.CreateContext("History_Recipient");
        var (senderId, recipientId) = await SeedUsers(context);
        var service = new TransactionService(context);

        await service.Transfer(senderId, new TransferRequest
        {
            RecipientAccountNumber = "2222222222",
            Amount = 5000
        });

        var result = await service.GetTransactionHistory(recipientId, 1, 10);

        Assert.Equal(200, result.Code);
        Assert.Single(result.Data!);
    }
}
