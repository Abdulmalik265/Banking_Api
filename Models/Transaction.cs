namespace BankingApi.Models;

public class Transaction
{
    public int Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public int SenderId { get; set; }
    public int RecipientId { get; set; }
    public decimal Amount { get; set; }
    public decimal SenderBalanceBefore { get; set; }
    public decimal SenderBalanceAfter { get; set; }
    public decimal RecipientBalanceBefore { get; set; }
    public decimal RecipientBalanceAfter { get; set; }
    public string Status { get; set; } = "Successful";
    public string? Narration { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual User Sender { get; set; } = null!;
    public virtual User Recipient { get; set; } = null!;
}
