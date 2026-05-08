using EagleBankAPI.Core.Entities.Enums;

namespace EagleBankAPI.Core.Entities;

public class BankAccount
{
    public string AccountNumber { get; set; } = string.Empty; // 01xxxxxx (8 digits)
    public string SortCode { get; set; } = "10-10-10";
    public string Name { get; set; } = string.Empty;
    public string AccountType { get; set; } = "personal";
    public decimal Balance { get; set; } = 0.00m;
    public Currency Currency { get; set; } = Currency.GBP;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedTimestamp { get; set; }
    public DateTime UpdatedTimestamp { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
