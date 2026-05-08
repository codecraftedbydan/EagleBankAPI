namespace EagleBankAPI.Models.Requests;

public class CreateTransactionRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "GBP";
    public string Type { get; set; } = string.Empty;
    public string? Reference { get; set; }
}
