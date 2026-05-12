using System.ComponentModel.DataAnnotations;

namespace EagleBankAPI.Models.Requests;

public class CreateTransactionRequest
{
    [Required]
    [Range(typeof(decimal), "0.00", "10000.00", ErrorMessage = "Amount must be between 0.00 and 10000.00")]
    public decimal? Amount { get; set; }

    [Required]
    [RegularExpression("^GBP$", ErrorMessage = "currency must be 'GBP'")]
    public string Currency { get; set; } = "GBP";

    [Required]
    [RegularExpression("^(deposit|withdrawal)$", ErrorMessage = "type must be 'deposit' or 'withdrawal'")]
    public string Type { get; set; } = string.Empty;

    public string? Reference { get; set; }
}
