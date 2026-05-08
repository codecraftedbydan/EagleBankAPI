using EagleBankAPI.Core.Exceptions;
using EagleBankAPI.Core.Entities;
using EagleBankAPI.Core.Entities.Enums;
using EagleBankAPI.Core.Repositories;
using EagleBankAPI.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace EagleBankAPI.Core.Services;

public class TransactionService : ITransactionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(IUnitOfWork unitOfWork, ILogger<TransactionService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Transaction> CreateTransactionAsync(string accountNumber, decimal amount, string type, string? reference, string requestingUserId)
    {
        _logger.LogInformation("Creating {TransactionType} transaction: Amount={Amount}, Account={AccountNumber}, User={UserId}", 
            type, amount, accountNumber, requestingUserId);
        
        var account = await _unitOfWork.BankAccounts.GetByAccountNumberAsync(accountNumber);
        if (account == null)
        {
            _logger.LogWarning("Transaction failed - account not found: {AccountNumber}", accountNumber);
            throw new NotFoundException("Bank account", accountNumber);
        }

        // Users can only create transactions for their own accounts
        if (account.UserId != requestingUserId)
        {
            throw new ForbiddenException("You can only create transactions for your own bank accounts");
        }

        // Parse and validate transaction type
        if (!Enum.TryParse<TransactionType>(type, true, out var transactionType))
        {
            throw new InvalidTransactionException($"Invalid transaction type. Must be 'Deposit' or 'Withdrawal'");
        }

        // Check for sufficient funds if withdrawal
        if (transactionType == TransactionType.Withdrawal && account.Balance < amount)
        {
            _logger.LogWarning("Withdrawal failed - insufficient funds. Requested: {RequestedAmount}, Available: {AvailableBalance}, Account: {AccountNumber}",
                amount, account.Balance, accountNumber);
            throw new InsufficientFundsException(amount, account.Balance);
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var transaction = new Transaction
            {
                Id = $"tan-{Guid.NewGuid():N}",
                Amount = amount,
                Currency = account.Currency,
                Type = transactionType,
                Reference = reference,
                AccountNumber = accountNumber,
                UserId = requestingUserId,
                CreatedTimestamp = DateTime.UtcNow
            };

            // Update account balance
            if (transactionType == TransactionType.Deposit)
            {
                account.Balance += amount;
            }
            else // withdrawal
            {
                account.Balance -= amount;
            }

            account.UpdatedTimestamp = DateTime.UtcNow;

            await _unitOfWork.Transactions.AddAsync(transaction);
            _unitOfWork.BankAccounts.Update(account);

            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Transaction completed successfully: {TransactionType} {Amount} {Currency}, Transaction ID: {TransactionId}, New Balance: {NewBalance}",
                transactionType, amount, account.Currency, transaction.Id, account.Balance);
            
            return transaction;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError("Transaction failed and rolled back: {TransactionType} {Amount} for account {AccountNumber}",
                type, amount, accountNumber);
            throw;
        }
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByAccountNumberAsync(string accountNumber, string requestingUserId)
    {
        var account = await _unitOfWork.BankAccounts.GetByAccountNumberAsync(accountNumber);
        if (account == null)
        {
            throw new NotFoundException("Bank account", accountNumber);
        }

        // Users can only view transactions for their own accounts
        if (account.UserId != requestingUserId)
        {
            throw new ForbiddenException("You can only view transactions for your own bank accounts");
        }

        return await _unitOfWork.Transactions.GetByAccountNumberAsync(accountNumber);
    }

    public async Task<Transaction?> GetTransactionByIdAsync(string accountNumber, string transactionId, string requestingUserId)
    {
        var account = await _unitOfWork.BankAccounts.GetByAccountNumberAsync(accountNumber);
        if (account == null)
        {
            throw new NotFoundException("Bank account", accountNumber);
        }

        // Users can only view transactions for their own accounts
        if (account.UserId != requestingUserId)
        {
            throw new ForbiddenException("You can only view transactions for your own bank accounts");
        }

        return await _unitOfWork.Transactions.GetByIdAndAccountNumberAsync(transactionId, accountNumber);
    }
}
