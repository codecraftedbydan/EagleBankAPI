namespace EagleBankAPI.Models.Responses;

public class TransactionResponse
{
    public string Id { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedTimestamp { get; set; }
}
