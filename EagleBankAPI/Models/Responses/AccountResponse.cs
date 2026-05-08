namespace EagleBankAPI.Models.Responses;

public class AccountResponse
{
    public string AccountNumber { get; set; } = string.Empty;
    public string SortCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime CreatedTimestamp { get; set; }
    public DateTime UpdatedTimestamp { get; set; }
}
