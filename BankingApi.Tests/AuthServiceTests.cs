using BankingApi.DTOs;
using BankingApi.Services;
using BankingApi.Tests.Helpers;
using Microsoft.Extensions.Configuration;

namespace BankingApi.Tests;

public class AuthServiceTests
{
    private readonly IConfiguration _config;

    public AuthServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:Key", "TestSecretKeyThatIsAtLeast32CharactersLong!" },
            { "Jwt:Issuer", "https://test.com" }
        };
        _config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsSuccess()
    {
        var context = TestDbHelper.CreateContext("Register_Valid");
        var service = new AuthService(context, _config);

        var result = await service.Register(new RegisterRequest
        {
            FullName = "John Doe",
            Email = "john@test.com",
            Password = "password123",
            Phone = "08012345678"
        });

        Assert.Equal(201, result.Code);
        Assert.NotNull(result.Data);
        Assert.Equal("john@test.com", result.Data!.User.Email);
        Assert.NotEmpty(result.Data.Token);
        Assert.NotEmpty(result.Data.User.AccountNumber);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequest()
    {
        var context = TestDbHelper.CreateContext("Register_Duplicate");
        var service = new AuthService(context, _config);

        await service.Register(new RegisterRequest
        {
            FullName = "User One",
            Email = "duplicate@test.com",
            Password = "password123"
        });

        var result = await service.Register(new RegisterRequest
        {
            FullName = "User Two",
            Email = "duplicate@test.com",
            Password = "password456"
        });

        Assert.Equal(400, result.Code);
        Assert.Contains("already registered", result.Message);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var context = TestDbHelper.CreateContext("Login_Valid");
        var service = new AuthService(context, _config);

        await service.Register(new RegisterRequest
        {
            FullName = "Jane Doe",
            Email = "jane@test.com",
            Password = "mypassword"
        });

        var result = await service.Login(new LoginRequest
        {
            Email = "jane@test.com",
            Password = "mypassword"
        });

        Assert.Equal(200, result.Code);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data!.Token);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var context = TestDbHelper.CreateContext("Login_WrongPassword");
        var service = new AuthService(context, _config);

        await service.Register(new RegisterRequest
        {
            FullName = "User",
            Email = "user@test.com",
            Password = "correctpassword"
        });

        var result = await service.Login(new LoginRequest
        {
            Email = "user@test.com",
            Password = "wrongpassword"
        });

        Assert.Equal(401, result.Code);
    }

    [Fact]
    public async Task Login_NonExistentEmail_ReturnsUnauthorized()
    {
        var context = TestDbHelper.CreateContext("Login_NoUser");
        var service = new AuthService(context, _config);

        var result = await service.Login(new LoginRequest
        {
            Email = "noone@test.com",
            Password = "password"
        });

        Assert.Equal(401, result.Code);
    }
}
