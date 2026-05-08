using Microsoft.EntityFrameworkCore;
using EagleBankAPI.Core.Entities;

namespace EagleBankAPI.DAL.Data;

public class EagleBankDbContext : DbContext
{
    public EagleBankDbContext(DbContextOptions<EagleBankDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<BankAccount> BankAccounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
        });

        // BankAccount configuration
        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.HasKey(e => e.AccountNumber);
            entity.Property(e => e.AccountNumber).HasMaxLength(8);
            entity.Property(e => e.SortCode).HasMaxLength(10);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Balance).HasColumnType("decimal(18,2)");
            
            entity.HasOne(e => e.User)
                  .WithMany(u => u.BankAccounts)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Transaction configuration
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Type).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Currency).HasMaxLength(3);
            
            entity.HasOne(e => e.BankAccount)
                  .WithMany(a => a.Transactions)
                  .HasForeignKey(e => e.AccountNumber)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
