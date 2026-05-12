using EagleBankAPI.Core.Repositories;
using EagleBankAPI.DAL.Data;
using Microsoft.EntityFrameworkCore.Storage;
namespace EagleBankAPI.DAL.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly EagleBankDbContext _context;
    private IDbContextTransaction? _transaction;

    public IUserRepository Users { get; }
    public IBankAccountRepository BankAccounts { get; }
    public ITransactionRepository Transactions { get; }

    public UnitOfWork(
        EagleBankDbContext context,
        IUserRepository userRepository,
        IBankAccountRepository bankAccountRepository,
        ITransactionRepository transactionRepository)
    {
        _context = context;
        Users = userRepository;
        BankAccounts = bankAccountRepository;
        Transactions = transactionRepository;
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        // In-memory database doesn't support transactions
        if (_context.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
