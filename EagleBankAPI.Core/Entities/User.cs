namespace EagleBankAPI.Core.Entities;

public class User
{
    public string Id { get; set; } = string.Empty; // usr-{guid}
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    
    // Address details
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string AddressTown { get; set; } = string.Empty;
    public string AddressCounty { get; set; } = string.Empty;
    public string AddressPostcode { get; set; } = string.Empty;
    
    public DateTime CreatedTimestamp { get; set; }
    public DateTime UpdatedTimestamp { get; set; }
    
    // Navigation properties
    public ICollection<BankAccount> BankAccounts { get; set; } = new List<BankAccount>();
}
