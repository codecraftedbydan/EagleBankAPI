namespace EagleBankAPI.Models.Requests;

public class CreateAccountRequest
{
    public string Name { get; set; } = string.Empty;
    public string AccountType { get; set; } = "personal";
}
