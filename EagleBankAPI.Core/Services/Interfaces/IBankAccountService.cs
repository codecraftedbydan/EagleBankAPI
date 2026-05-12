using EagleBankAPI.Core.Entities;

namespace EagleBankAPI.Core.Services.Interfaces;

public interface IBankAccountService
{
    Task<BankAccount> CreateAccountAsync(string name, string accountType, string userId);
    Task<IEnumerable<BankAccount>> GetAccountsByUserIdAsync(string userId);
    Task<BankAccount?> GetAccountByNumberAsync(string accountNumber, string requestingUserId);
    Task<BankAccount> UpdateAccountAsync(string accountNumber, string name, string requestingUserId);
    Task DeleteAccountAsync(string accountNumber, string requestingUserId);
}
