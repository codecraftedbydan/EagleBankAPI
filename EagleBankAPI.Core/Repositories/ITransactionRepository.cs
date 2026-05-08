using EagleBankAPI.Core.Entities;

namespace EagleBankAPI.Core.Repositories;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<IEnumerable<Transaction>> GetByAccountNumberAsync(string accountNumber);
    Task<Transaction?> GetByIdAndAccountNumberAsync(string transactionId, string accountNumber);
}
