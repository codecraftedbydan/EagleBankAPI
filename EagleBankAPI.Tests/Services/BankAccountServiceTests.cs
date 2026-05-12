using EagleBankAPI.Core.Entities;
using EagleBankAPI.Core.Entities.Enums;
using EagleBankAPI.Core.Exceptions;
using EagleBankAPI.Core.Repositories;
using EagleBankAPI.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EagleBankAPI.Tests.Services;

public class BankAccountServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<BankAccountService>> _loggerMock;
    private readonly BankAccountService _bankAccountService;

    public BankAccountServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<BankAccountService>>();
        _bankAccountService = new BankAccountService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAccountAsync_WithValidData_ShouldCreateAccount()
    {
        // Arrange
        var userId = "usr-123";
        var user = new User { Id = userId };
        var accountName = "Savings Account";
        var accountType = "personal";

        _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _unitOfWorkMock.Setup(u => u.BankAccounts.GenerateAccountNumberAsync())
            .ReturnsAsync("01234567");
        _unitOfWorkMock.Setup(u => u.BankAccounts.AddAsync(It.IsAny<BankAccount>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CompleteAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _bankAccountService.CreateAccountAsync(accountName, accountType, userId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(accountName);
        result.AccountNumber.Should().Be("01234567");
        result.SortCode.Should().Be("10-10-10");
        result.Balance.Should().Be(0.00m);
        result.Currency.Should().Be(Currency.GBP);
        result.AccountType.Should().Be(accountType);

        _unitOfWorkMock.Verify(u => u.BankAccounts.AddAsync(It.IsAny<BankAccount>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAccountAsync_WhenUserDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var userId = "usr-123";
        var accountName = "Account";

        _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _bankAccountService.CreateAccountAsync(accountName, "personal", userId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*User*not found*");
    }

    [Fact]
    public async Task GetAccountsByUserIdAsync_ShouldReturnUserAccounts()
    {
        // Arrange
        var userId = "usr-123";
        var accounts = new List<BankAccount>
        {
            new BankAccount
            {
                AccountNumber = "01234567",
                Name = "Account 1",
                UserId = userId,
                Balance = 100m,
                SortCode = "10-10-10",
                AccountType = "personal",
                Currency = Currency.GBP,
                CreatedTimestamp = DateTime.UtcNow,
                UpdatedTimestamp = DateTime.UtcNow
            },
            new BankAccount
            {
                AccountNumber = "01234568",
                Name = "Account 2",
                UserId = userId,
                Balance = 200m,
                SortCode = "10-10-10",
                AccountType = "personal",
                Currency = Currency.GBP,
                CreatedTimestamp = DateTime.UtcNow,
                UpdatedTimestamp = DateTime.UtcNow
            }
        };

        _unitOfWorkMock.Setup(u => u.BankAccounts.GetByUserIdAsync(userId))
            .ReturnsAsync(accounts);

        // Act
        var result = await _bankAccountService.GetAccountsByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        var accountList = result.ToList();
        accountList.Should().HaveCount(2);
        accountList[0].AccountNumber.Should().Be("01234567");
        accountList[1].AccountNumber.Should().Be("01234568");
    }

    [Fact]
    public async Task GetAccountByNumberAsync_WhenAccessingOwnAccount_ShouldReturnAccount()
    {
        // Arrange
        var userId = "usr-123";
        var accountNumber = "01234567";
        var account = new BankAccount
        {
            AccountNumber = accountNumber,
            Name = "My Account",
            UserId = userId,
            Balance = 1000m,
            SortCode = "10-10-10",
            AccountType = "personal",
            Currency = Currency.GBP,
            CreatedTimestamp = DateTime.UtcNow,
            UpdatedTimestamp = DateTime.UtcNow
        };

        _unitOfWorkMock.Setup(u => u.BankAccounts.GetByAccountNumberAsync(accountNumber))
            .ReturnsAsync(account);

        // Act
        var result = await _bankAccountService.GetAccountByNumberAsync(accountNumber, userId);

        // Assert
        result.Should().NotBeNull();
        result!.AccountNumber.Should().Be(accountNumber);
        result.Balance.Should().Be(1000m);
    }

    [Fact]
    public async Task GetAccountByNumberAsync_WhenAccessingAnotherUsersAccount_ShouldThrowException()
    {
        // Arrange
        var accountNumber = "01234567";
        var ownerId = "usr-123";
        var requestingUserId = "usr-456";
        var account = new BankAccount
        {
            AccountNumber = accountNumber,
            UserId = ownerId
        };

        _unitOfWorkMock.Setup(u => u.BankAccounts.GetByAccountNumberAsync(accountNumber))
            .ReturnsAsync(account);

        // Act
        Func<Task> act = async () => await _bankAccountService.GetAccountByNumberAsync(accountNumber, requestingUserId);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("You can only view your own bank accounts");
    }

    [Fact]
    public async Task UpdateAccountAsync_WithValidData_ShouldUpdateAccount()
    {
        // Arrange
        var userId = "usr-123";
        var accountNumber = "01234567";
        var account = new BankAccount
        {
            AccountNumber = accountNumber,
            Name = "Old Name",
            UserId = userId,
            AccountType = "personal",
            Balance = 100m,
            SortCode = "10-10-10",
            Currency = Currency.GBP,
            CreatedTimestamp = DateTime.UtcNow,
            UpdatedTimestamp = DateTime.UtcNow
        };

        var newName = "New Name";

        _unitOfWorkMock.Setup(u => u.BankAccounts.GetByAccountNumberAsync(accountNumber))
            .ReturnsAsync(account);
        _unitOfWorkMock.Setup(u => u.CompleteAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _bankAccountService.UpdateAccountAsync(accountNumber, newName, null, userId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Name");

        _unitOfWorkMock.Verify(u => u.BankAccounts.Update(It.IsAny<BankAccount>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAccountAsync_WhenAccountExists_ShouldDeleteAccount()
    {
        // Arrange
        var userId = "usr-123";
        var accountNumber = "01234567";
        var account = new BankAccount
        {
            AccountNumber = accountNumber,
            UserId = userId
        };

        _unitOfWorkMock.Setup(u => u.BankAccounts.GetByAccountNumberAsync(accountNumber))
            .ReturnsAsync(account);
        _unitOfWorkMock.Setup(u => u.CompleteAsync())
            .ReturnsAsync(1);

        // Act
        await _bankAccountService.DeleteAccountAsync(accountNumber, userId);

        // Assert
        _unitOfWorkMock.Verify(u => u.BankAccounts.Remove(account), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }
}
