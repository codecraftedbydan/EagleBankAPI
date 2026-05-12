using EagleBankAPI.Core.Entities;
using EagleBankAPI.Core.Repositories;
using EagleBankAPI.DAL.Data;
using Microsoft.EntityFrameworkCore;
namespace EagleBankAPI.DAL.Repositories;

public class BankAccountRepository : Repository<BankAccount>, IBankAccountRepository
{
    public BankAccountRepository(EagleBankDbContext context) : base(context)
    {
    }

    public async Task<BankAccount?> GetByAccountNumberAsync(string accountNumber)
    {
        return await _dbSet
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
    }

    public async Task<IEnumerable<BankAccount>> GetByUserIdAsync(string userId)
    {
        return await _dbSet
            .Where(a => a.UserId == userId)
            .ToListAsync();
    }

    public async Task<string> GenerateAccountNumberAsync()
    {
        // Generate account number: 01XXXXXX (8 digits total)
        var random = new Random();
        string accountNumber;

        do
        {
            var randomPart = random.Next(100000, 1000000); // 6 digits
            accountNumber = $"01{randomPart}";
        }
        while (await _dbSet.AnyAsync(a => a.AccountNumber == accountNumber));

        return accountNumber;
    }
}
