namespace EagleBankAPI.Core.Repositories;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IBankAccountRepository BankAccounts { get; }
    ITransactionRepository Transactions { get; }
    Task<int> CompleteAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
