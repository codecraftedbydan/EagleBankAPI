using System.ComponentModel.DataAnnotations;

namespace EagleBankAPI.Models.Requests;

public class CreateAccountRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^personal$", ErrorMessage = "accountType must be 'personal'")]
    public string AccountType { get; set; } = "personal";
}
