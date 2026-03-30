using BankingApi.DTOs;
using BankingApi.Models;
using BankingApi.Services;
using BankingApi.Tests.Helpers;

namespace BankingApi.Tests;

public class AccountServiceTests
{
    private async Task<int> SeedUser(BankingApi.Data.BankingDbContext context)
    {
        var user = new User
        {
            FullName = "Test User",
            Email = "test@test.com",
            PasswordHash = "hash",
            Phone = "0801234",
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var account = new Account
        {
            UserId = user.Id,
            AccountNumber = "1234567890",
            Balance = 10000,
            Currency = "NGN",
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };
        context.Accounts.Add(account);
        await context.SaveChangesAsync();
        return user.Id;
    }

    [Fact]
    public async Task GetAccountInfo_ValidUser_ReturnsAccount()
    {
        var context = TestDbHelper.CreateContext("GetAccount_Valid");
        var userId = await SeedUser(context);
        var service = new AccountService(context);

        var result = await service.GetAccountInfo(userId);

        Assert.Equal(200, result.Code);
        Assert.Equal("1234567890", result.Data!.AccountNumber);
        Assert.Equal(10000, result.Data.Balance);
    }

    [Fact]
    public async Task GetAccountInfo_InvalidUser_ReturnsNotFound()
    {
        var context = TestDbHelper.CreateContext("GetAccount_Invalid");
        var service = new AccountService(context);

        var result = await service.GetAccountInfo(999);

        Assert.Equal(404, result.Code);
    }

    [Fact]
    public async Task UpdateAccount_ChangeName_ReturnsUpdated()
    {
        var context = TestDbHelper.CreateContext("UpdateAccount_Name");
        var userId = await SeedUser(context);
        var service = new AccountService(context);

        var result = await service.UpdateAccount(userId, new UpdateAccountRequest
        {
            FullName = "Updated Name"
        });

        Assert.Equal(200, result.Code);
        Assert.Equal("Updated Name", result.Data!.FullName);
    }

    [Fact]
    public async Task UpdateAccount_ChangePhone_ReturnsUpdated()
    {
        var context = TestDbHelper.CreateContext("UpdateAccount_Phone");
        var userId = await SeedUser(context);
        var service = new AccountService(context);

        var result = await service.UpdateAccount(userId, new UpdateAccountRequest
        {
            Phone = "09099999999"
        });

        Assert.Equal(200, result.Code);
        Assert.Equal("09099999999", result.Data!.Phone);
    }

    [Fact]
    public async Task UpdateAccount_InvalidUser_ReturnsNotFound()
    {
        var context = TestDbHelper.CreateContext("UpdateAccount_InvalidUser");
        var service = new AccountService(context);

        var result = await service.UpdateAccount(999, new UpdateAccountRequest
        {
            FullName = "Ghost"
        });

        Assert.Equal(404, result.Code);
    }

    [Fact]
    public async Task UpdateAccount_EmptyFields_KeepsOriginalValues()
    {
        var context = TestDbHelper.CreateContext("UpdateAccount_Empty");
        var userId = await SeedUser(context);
        var service = new AccountService(context);

        var result = await service.UpdateAccount(userId, new UpdateAccountRequest
        {
            FullName = "   ",
            Phone = ""
        });

        Assert.Equal(200, result.Code);
        Assert.Equal("Test User", result.Data!.FullName);
        Assert.Equal("0801234", result.Data.Phone);
    }
}
