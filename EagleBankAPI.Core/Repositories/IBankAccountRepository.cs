using EagleBankAPI.Core.Entities;

namespace EagleBankAPI.Core.Repositories;

public interface IBankAccountRepository : IRepository<BankAccount>
{
    Task<BankAccount?> GetByAccountNumberAsync(string accountNumber);
    Task<IEnumerable<BankAccount>> GetByUserIdAsync(string userId);
    Task<string> GenerateAccountNumberAsync();
}
