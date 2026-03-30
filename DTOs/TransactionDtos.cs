using System.ComponentModel.DataAnnotations;

namespace BankingApi.DTOs;

public class TransferRequest
{
    [Required, StringLength(10)]
    public string RecipientAccountNumber { get; set; } = string.Empty;

    [Required, Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [StringLength(200)]
    public string? Narration { get; set; }
}

public class TransactionResponse
{
    public int Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderAccount { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientAccount { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal SenderBalanceBefore { get; set; }
    public decimal SenderBalanceAfter { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Narration { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ApiResponse<T>
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public ApiResponse(int code, string message, T? data = default)
    {
        Code = code;
        Message = message;
        Data = data;
    }
}
