using EagleBankAPI.Core.Exceptions;
using EagleBankAPI.Core.Entities;
using EagleBankAPI.Core.Entities.Enums;
using EagleBankAPI.Core.Repositories;
using EagleBankAPI.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace EagleBankAPI.Core.Services;

public class BankAccountService : IBankAccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BankAccountService> _logger;

    public BankAccountService(IUnitOfWork unitOfWork, ILogger<BankAccountService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BankAccount> CreateAccountAsync(string name, string currency, string userId)
    {
        _logger.LogInformation("Creating bank account for user: {UserId}, Name: {AccountName}", userId, name);
        
        // Verify user exists
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Account creation failed - user not found: {UserId}", userId);
            throw new NotFoundException("User", userId);
        }

        // Parse currency
        if (!Enum.TryParse<Currency>(currency, true, out var currencyEnum))
        {
            currencyEnum = Currency.GBP;
        }

        var accountNumber = await _unitOfWork.BankAccounts.GenerateAccountNumberAsync();

        var account = new BankAccount
        {
            AccountNumber = accountNumber,
            SortCode = "10-10-10",
            Name = name,
            AccountType = "personal",
            Balance = 0.00m,
            Currency = currencyEnum,
            UserId = userId,
            CreatedTimestamp = DateTime.UtcNow,
            UpdatedTimestamp = DateTime.UtcNow
        };

        await _unitOfWork.BankAccounts.AddAsync(account);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Bank account created successfully: {AccountNumber} for user: {UserId}", accountNumber, userId);
        return account;
    }

    public async Task<IEnumerable<BankAccount>> GetAccountsByUserIdAsync(string userId)
    {
        return await _unitOfWork.BankAccounts.GetByUserIdAsync(userId);
    }

    public async Task<BankAccount?> GetAccountByNumberAsync(string accountNumber, string requestingUserId)
    {
        var account = await _unitOfWork.BankAccounts.GetByAccountNumberAsync(accountNumber);
        if (account == null)
        {
            return null;
        }

        // Users can only view their own accounts
        if (account.UserId != requestingUserId)
        {
            throw new ForbiddenException("You can only view your own bank accounts");
        }

        return account;
    }

    public async Task<BankAccount> UpdateAccountAsync(string accountNumber, string name, string requestingUserId)
    {
        var account = await _unitOfWork.BankAccounts.GetByAccountNumberAsync(accountNumber);
        if (account == null)
        {
            throw new NotFoundException("Bank account", accountNumber);
        }

        // Users can only update their own accounts
        if (account.UserId != requestingUserId)
        {
            throw new ForbiddenException("You can only update your own bank accounts");
        }

        account.Name = name;
        account.UpdatedTimestamp = DateTime.UtcNow;

        _unitOfWork.BankAccounts.Update(account);
        await _unitOfWork.CompleteAsync();

        return account;
    }

    public async Task DeleteAccountAsync(string accountNumber, string requestingUserId)
    {
        var account = await _unitOfWork.BankAccounts.GetByAccountNumberAsync(accountNumber);
        if (account == null)
        {
            throw new NotFoundException("Bank account", accountNumber);
        }

        // Users can only delete their own accounts
        if (account.UserId != requestingUserId)
        {
            throw new ForbiddenException("You can only delete your own bank accounts");
        }

        _unitOfWork.BankAccounts.Remove(account);
        await _unitOfWork.CompleteAsync();
        
        _logger.LogInformation("Bank account deleted: {AccountNumber} by user: {UserId}", accountNumber, requestingUserId);
    }
}
