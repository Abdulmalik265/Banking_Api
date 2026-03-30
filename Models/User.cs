namespace BankingApi.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;
    public virtual ICollection<Transaction> SentTransactions { get; set; } = new List<Transaction>();
    public virtual ICollection<Transaction> ReceivedTransactions { get; set; } = new List<Transaction>();
}
