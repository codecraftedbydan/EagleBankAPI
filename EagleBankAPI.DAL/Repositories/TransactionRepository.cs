using Microsoft.EntityFrameworkCore;
using EagleBankAPI.DAL.Data;
using EagleBankAPI.Core.Entities;

using EagleBankAPI.Core.Repositories;
namespace EagleBankAPI.DAL.Repositories;

public class TransactionRepository : Repository<Transaction>, ITransactionRepository
{
    public TransactionRepository(EagleBankDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Transaction>> GetByAccountNumberAsync(string accountNumber)
    {
        return await _dbSet
            .Where(t => t.AccountNumber == accountNumber)
            .OrderByDescending(t => t.CreatedTimestamp)
            .ToListAsync();
    }

    public async Task<Transaction?> GetByIdAndAccountNumberAsync(string transactionId, string accountNumber)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.Id == transactionId && t.AccountNumber == accountNumber);
    }
}
