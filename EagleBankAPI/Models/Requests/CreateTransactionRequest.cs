using System.ComponentModel.DataAnnotations;

namespace EagleBankAPI.Models.Requests;

public class CreateTransactionRequest
{
    [Required]
    public decimal Amount { get; set; }

    [Required]
    public string Currency { get; set; } = "GBP";

    [Required]
    public string Type { get; set; } = string.Empty;

    public string? Reference { get; set; }
}
