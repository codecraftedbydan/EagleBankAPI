using System.ComponentModel.DataAnnotations;

namespace EagleBankAPI.Models.Requests;

public class CreateAccountRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string AccountType { get; set; } = "personal";
}
