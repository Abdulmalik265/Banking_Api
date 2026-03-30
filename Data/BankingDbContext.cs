using BankingApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BankingApi.Data;

public class BankingDbContext : DbContext
{
    public BankingDbContext(DbContextOptions<BankingDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasOne(e => e.Account).WithOne(a => a.User).HasForeignKey<Account>(a => a.UserId);
        });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("accounts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.AccountNumber).HasColumnName("account_number").HasMaxLength(10).IsRequired();
            entity.Property(e => e.Balance).HasColumnName("balance").HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasColumnName("currency").HasMaxLength(5);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => e.AccountNumber).IsUnique();
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("transactions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Reference).HasColumnName("reference").HasMaxLength(50).IsRequired();
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.RecipientId).HasColumnName("recipient_id");
            entity.Property(e => e.Amount).HasColumnName("amount").HasPrecision(18, 2);
            entity.Property(e => e.SenderBalanceBefore).HasColumnName("sender_balance_before").HasPrecision(18, 2);
            entity.Property(e => e.SenderBalanceAfter).HasColumnName("sender_balance_after").HasPrecision(18, 2);
            entity.Property(e => e.RecipientBalanceBefore).HasColumnName("recipient_balance_before").HasPrecision(18, 2);
            entity.Property(e => e.RecipientBalanceAfter).HasColumnName("recipient_balance_after").HasPrecision(18, 2);
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(20);
            entity.Property(e => e.Narration).HasColumnName("narration").HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(e => e.Reference).IsUnique();

            entity.HasOne(e => e.Sender).WithMany(u => u.SentTransactions)
                .HasForeignKey(e => e.SenderId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Recipient).WithMany(u => u.ReceivedTransactions)
                .HasForeignKey(e => e.RecipientId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
