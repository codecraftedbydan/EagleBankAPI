using EagleBankAPI.Core.Entities.Enums;

namespace EagleBankAPI.Core.Entities;

public class Transaction
{
    public string Id { get; set; } = string.Empty; // tan-{guid}
    public decimal Amount { get; set; }
    public Currency Currency { get; set; } = Currency.GBP;
    public TransactionType Type { get; set; }
    public string? Reference { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedTimestamp { get; set; }

    // Navigation property
    public BankAccount BankAccount { get; set; } = null!;
}
