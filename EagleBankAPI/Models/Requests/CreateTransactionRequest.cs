using System.ComponentModel.DataAnnotations;

namespace EagleBankAPI.Models.Requests;

public class CreateTransactionRequest
{
    [Required]
    [Range(typeof(decimal), "0.00", "10000.00", ErrorMessage = "Amount must be between 0.00 and 10000.00")]
    public decimal? Amount { get; set; }

    [Required]
    public string Currency { get; set; } = "GBP";

    [Required]
    public string Type { get; set; } = string.Empty;

    public string? Reference { get; set; }
}
