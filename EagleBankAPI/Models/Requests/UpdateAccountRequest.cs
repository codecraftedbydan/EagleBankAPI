using System.ComponentModel.DataAnnotations;

namespace EagleBankAPI.Models.Requests;

public class UpdateAccountRequest
{
    public string? Name { get; set; }

    [RegularExpression("^personal$", ErrorMessage = "accountType must be 'personal'")]
    public string? AccountType { get; set; }
}
