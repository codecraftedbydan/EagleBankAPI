using FluentAssertions;
using Moq;
using Xunit;
using EagleBankAPI.Core.Entities;
using EagleBankAPI.Core.Entities.Enums;
using EagleBankAPI.Core.Repositories;
using EagleBankAPI.Core.Services;
using EagleBankAPI.Core.Services.Interfaces;
using EagleBankAPI.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace EagleBankAPI.Tests.Services;

public class TransactionServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<TransactionService>> _loggerMock;
    private readonly TransactionService _transactionService;

    public TransactionServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<TransactionService>>();
        _transactionService = new TransactionService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateTransactionAsync_Deposit_ShouldIncreaseBalance()
    {
        // Arrange
        var userId = "usr-123";
        var accountNumber = "01234567";
        var account = new BankAccount
        {
            AccountNumber = accountNumber,
            UserId = userId,
            Balance = 100m,
            Name = "Account",
            SortCode = "10-10-10",
            AccountType = "personal",
            Currency = Currency.GBP,
            CreatedTimestamp = DateTime.UtcNow,
            UpdatedTimestamp = DateTime.UtcNow
        };

        var amount = 50m;
        var type = "Deposit";
        var reference = "Test deposit";

        _unitOfWorkMock.Setup(u => u.BankAccounts.GetByAccountNumberAsync(accountNumber))
            .ReturnsAsync(account);
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.Transactions.AddAsync(It.IsAny<Transaction>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _transactionService.CreateTransactionAsync(accountNumber, amount, type, reference, userId);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(50m);
        result.Type.Should().Be(TransactionType.Deposit);
        result.Id.Should().StartWith("tan-");
        account.Balance.Should().Be(150m); // Original 100 + 50 deposit

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
        _unitOfWorkMock.Verify(u => u.Transactions.AddAsync(It.IsAny<Transaction>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.BankAccounts.Update(account), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateTransactionAsync_Withdrawal_ShouldDecreaseBalance()
    {
        // Arrange
        var userId = "usr-123";
        var accountNumber = "01234567";
        var account = new BankAccount
        {
            AccountNumber = accountNumber,
            UserId = userId,
            Balance = 100m,
            Name = "Account",
            SortCode = "10-10-10",
            AccountType = "personal",
            Currency = Currency.GBP,
            CreatedTimestamp = DateTime.UtcNow,
            UpdatedTimestamp = DateTime.UtcNow
        };

        var amount = 30m;
        var type = "Withdrawal";
        var reference = "Test withdrawal";

        _unitOfWorkMock.Setup(u => u.BankAccounts.GetByAccountNumberAsync(accountNumber))
            .ReturnsAsync(account);
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync())
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.Transactions.AddAsync(It.IsAny<Transaction>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _transactionService.CreateTransactionAsync(accountNumber, amount, type, reference, userId);

        // Assert
        result.Should().NotBeNull();
        result.Amount.Should().Be(30m);
        result.Type.Should().Be(TransactionType.Withdrawal);
        account.Balance.Should().Be(70m); // Original 100 - 30 withdrawal

        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateTransactionAsync_WithInsufficientFunds_ShouldThrowException()
    {
        // Arrange
        var userId = "usr-123";
        var accountNumber = "01234567";
        var account = new BankAccount
        {
            AccountNumber = accountNumber,
            UserId = userId,
            Balance = 50m
        };

        var amount = 100m;
        var type = "Withdrawal";

        _unitOfWorkMock.Setup(u => u.BankAccounts.GetByAccountNumberAsync(accountNumber))
            .ReturnsAsync(account);

        // Act
        Func<Task> act = async () => await _transactionService.CreateTransactionAsync(accountNumber, amount, type, null, userId);

        // Assert
        await act.Should().ThrowAsync<InsufficientFundsException>()
            .WithMessage("Insufficient funds*");
    }

    [Fact]
    public async Task CreateTransactionAsync_ForAnotherUsersAccount_ShouldThrowException()
    {
        // Arrange
        var ownerId = "usr-123";
        var requestingUserId = "usr-456";
        var accountNumber = "01234567";
        var account = new BankAccount
        {
            AccountNumber = accountNumber,
            UserId = ownerId,
            Balance = 100m
        };

        var amount = 10m;
        var type = "Deposit";

        _unitOfWorkMock.Setup(u => u.BankAccounts.GetByAccountNumberAsync(accountNumber))
            .ReturnsAsync(account);

        // Act
        Func<Task> act = async () => await _transactionService.CreateTransactionAsync(accountNumber, amount, type, null, requestingUserId);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("You can only create transactions for your own bank accounts");
    }

    [Fact]
    public async Task CreateTransactionAsync_WithInvalidType_ShouldThrowException()
    {
        // Arrange
        var userId = "usr-123";
        var accountNumber = "01234567";
        var account = new BankAccount
        {
            AccountNumber = accountNumber,
            UserId = userId,
            Balance = 100m
        };

        var amount = 10m;
        var type = "Invalid"; // Invalid type value

        _unitOfWorkMock.Setup(u => u.BankAccounts.GetByAccountNumberAsync(accountNumber))
            .ReturnsAsync(account);

        // Act
        Func<Task> act = async () => await _transactionService.CreateTransactionAsync(accountNumber, amount, type, null, userId);

        // Assert
        await act.Should().ThrowAsync<InvalidTransactionException>()
            .WithMessage("*Invalid transaction type*");
    }

    [Fact]
    public async Task GetTransactionsByAccountNumberAsync_ShouldReturnTransactions()
    {
        // Arrange
        var userId = "usr-123";
        var accountNumber = "01234567";
        var account = new BankAccount
        {
            AccountNumber = accountNumber,
            UserId = userId
        };

        var transactions = new List<Transaction>
        {
            new Transaction
            {
                Id = "tan-1",
                Amount = 100m,
                Type = TransactionType.Deposit,
                Currency = Currency.GBP,
                AccountNumber = accountNumber,
                UserId = userId,
                CreatedTimestamp = DateTime.UtcNow
            },
            new Transaction
            {
                Id = "tan-2",
                Amount = 50m,
                Type = TransactionType.Withdrawal,
                Currency = Currency.GBP,
                AccountNumber = accountNumber,
                UserId = userId,
                CreatedTimestamp = DateTime.UtcNow
            }
        };

        _unitOfWorkMock.Setup(u => u.BankAccounts.GetByAccountNumberAsync(accountNumber))
            .ReturnsAsync(account);
        _unitOfWorkMock.Setup(u => u.Transactions.GetByAccountNumberAsync(accountNumber))
            .ReturnsAsync(transactions);

        // Act
        var result = await _transactionService.GetTransactionsByAccountNumberAsync(accountNumber, userId);

        // Assert
        result.Should().NotBeNull();
        var transactionList = result.ToList();
        transactionList.Should().HaveCount(2);
        transactionList[0].Id.Should().Be("tan-1");
        transactionList[0].Type.Should().Be(TransactionType.Deposit);
        transactionList[1].Id.Should().Be("tan-2");
        transactionList[1].Type.Should().Be(TransactionType.Withdrawal);
    }

    [Fact]
    public async Task GetTransactionByIdAsync_WhenExists_ShouldReturnTransaction()
    {
        // Arrange
        var userId = "usr-123";
        var accountNumber = "01234567";
        var transactionId = "tan-123";
        var account = new BankAccount
        {
            AccountNumber = accountNumber,
            UserId = userId
        };

        var transaction = new Transaction
        {
            Id = transactionId,
            Amount = 100m,
            Type = TransactionType.Deposit,
            Currency = Currency.GBP,
            AccountNumber = accountNumber,
            UserId = userId,
            CreatedTimestamp = DateTime.UtcNow
        };

        _unitOfWorkMock.Setup(u => u.BankAccounts.GetByAccountNumberAsync(accountNumber))
            .ReturnsAsync(account);
        _unitOfWorkMock.Setup(u => u.Transactions.GetByIdAndAccountNumberAsync(transactionId, accountNumber))
            .ReturnsAsync(transaction);

        // Act
        var result = await _transactionService.GetTransactionByIdAsync(accountNumber, transactionId, userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(transactionId);
        result.Amount.Should().Be(100m);
        result.Type.Should().Be(TransactionType.Deposit);
    }
}
