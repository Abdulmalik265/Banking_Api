using BankingApi.Data;
using BankingApi.DTOs;
using BankingApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BankingApi.Services;

public class AuthService
{
    private readonly BankingDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(BankingDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<ApiResponse<AuthResponse>> Register(RegisterRequest request)
    {
        var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email.Trim().ToLower());
        if (emailExists)
            return new ApiResponse<AuthResponse>(400, "Email already registered");

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Phone = request.Phone?.Trim() ?? "",
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Create account with generated account number
        var account = new Account
        {
            UserId = user.Id,
            AccountNumber = GenerateAccountNumber(),
            Balance = 0,
            Currency = "NGN",
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };

        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);

        return new ApiResponse<AuthResponse>(201, "Registration successful", new AuthResponse
        {
            Token = token.Token,
            Expires = token.Expires,
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance
            }
        });
    }

    public async Task<ApiResponse<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Account)
            .FirstOrDefaultAsync(u => u.Email == request.Email.Trim().ToLower());

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return new ApiResponse<AuthResponse>(401, "Invalid email or password");

        var token = GenerateJwtToken(user);

        return new ApiResponse<AuthResponse>(200, "Login successful", new AuthResponse
        {
            Token = token.Token,
            Expires = token.Expires,
            User = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                AccountNumber = user.Account?.AccountNumber ?? "",
                Balance = user.Account?.Balance ?? 0
            }
        });
    }

    private (string Token, DateTime Expires) GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(24);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Issuer"],
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    private string GenerateAccountNumber()
    {
        var random = new Random();
        string accountNumber;
        do
        {
            accountNumber = random.Next(1000000000, int.MaxValue).ToString()[..10];
        } while (_context.Accounts.Any(a => a.AccountNumber == accountNumber));

        return accountNumber;
    }
}
