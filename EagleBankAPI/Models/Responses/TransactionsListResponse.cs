namespace EagleBankAPI.Models.Responses;

public class TransactionsListResponse
{
    public List<TransactionResponse> Transactions { get; set; } = new();
}
