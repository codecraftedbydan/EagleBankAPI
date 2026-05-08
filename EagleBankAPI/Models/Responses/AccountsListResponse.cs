namespace EagleBankAPI.Models.Responses;

public class AccountsListResponse
{
    public List<AccountResponse> Accounts { get; set; } = new();
}
