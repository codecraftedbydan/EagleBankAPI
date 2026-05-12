using EagleBankAPI.Core.Entities;

namespace EagleBankAPI.Core.Services.Interfaces;

public interface ITransactionService
{
    Task<Transaction> CreateTransactionAsync(string accountNumber, decimal amount, string currency, string type, string? reference, string requestingUserId);
    Task<IEnumerable<Transaction>> GetTransactionsByAccountNumberAsync(string accountNumber, string requestingUserId);
    Task<Transaction?> GetTransactionByIdAsync(string accountNumber, string transactionId, string requestingUserId);
}
