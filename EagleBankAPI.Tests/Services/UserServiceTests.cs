using EagleBankAPI.Core.Entities;
using EagleBankAPI.Core.Exceptions;
using EagleBankAPI.Core.Repositories;
using EagleBankAPI.Core.Services;
using EagleBankAPI.Core.Services.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EagleBankAPI.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<UserService>>();
        _userService = new UserService(_unitOfWorkMock.Object, _authServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateUserAsync_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var name = "John Doe";
        var email = "john.doe@example.com";
        var password = "SecurePassword123!";
        var phoneNumber = "+441234567890";
        var addressLine1 = "123 Main St";
        var town = "London";
        var county = "Greater London";
        var postcode = "SW1A 1AA";

        _unitOfWorkMock.Setup(u => u.Users.EmailExistsAsync(email))
            .ReturnsAsync(false);
        _authServiceMock.Setup(a => a.HashPassword(password))
            .Returns("hashedPassword");
        _unitOfWorkMock.Setup(u => u.Users.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CompleteAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _userService.CreateUserAsync(name, email, password, phoneNumber, addressLine1, null, null, town, county, postcode);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(name);
        result.Email.Should().Be(email);
        result.PhoneNumber.Should().Be(phoneNumber);
        result.AddressLine1.Should().Be(addressLine1);
        result.Id.Should().StartWith("usr-");

        _unitOfWorkMock.Verify(u => u.Users.AddAsync(It.IsAny<User>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WithExistingEmail_ShouldThrowException()
    {
        // Arrange
        var email = "existing@example.com";
        var password = "password";

        _unitOfWorkMock.Setup(u => u.Users.EmailExistsAsync(email))
            .ReturnsAsync(true);

        // Act
        Func<Task> act = async () => await _userService.CreateUserAsync(
            "Name", email, password, "+441234567890", "Line1", null, null, "Town", "County", "Postcode");

        // Assert
        await act.Should().ThrowAsync<DuplicateEmailException>()
            .WithMessage("Email 'existing@example.com' is already registered");
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var userId = "usr-123";
        var user = new User
        {
            Id = userId,
            Name = "John Doe",
            Email = "john@example.com",
            PhoneNumber = "+441234567890",
            AddressLine1 = "123 Main St",
            AddressTown = "London",
            AddressCounty = "Greater London",
            AddressPostcode = "SW1A 1AA",
            CreatedTimestamp = DateTime.UtcNow,
            UpdatedTimestamp = DateTime.UtcNow
        };

        _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.GetUserByIdAsync(userId, userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.Name.Should().Be(user.Name);
        result.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetUserByIdAsync_WhenAccessingAnotherUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var userId = "usr-123";
        var requestingUserId = "usr-456";

        // Act
        Func<Task> act = async () => await _userService.GetUserByIdAsync(userId, requestingUserId);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("You can only view your own user details");
    }

    [Fact]
    public async Task UpdateUserAsync_WithValidData_ShouldUpdateUser()
    {
        // Arrange
        var userId = "usr-123";
        var user = new User
        {
            Id = userId,
            Name = "John Doe",
            Email = "john@example.com",
            PhoneNumber = "+441234567890",
            AddressLine1 = "123 Main St",
            AddressTown = "London",
            AddressCounty = "Greater London",
            AddressPostcode = "SW1A 1AA"
        };

        var updatedName = "John Updated";
        var updatedPhoneNumber = "+449876543210";

        _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _unitOfWorkMock.Setup(u => u.CompleteAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _userService.UpdateUserAsync(userId, updatedName, user.Email, updatedPhoneNumber,
            user.AddressLine1, null, null, user.AddressTown, user.AddressCounty, user.AddressPostcode, userId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(updatedName);
        result.PhoneNumber.Should().Be(updatedPhoneNumber);

        _unitOfWorkMock.Verify(u => u.Users.Update(It.IsAny<User>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_WhenUserHasNoAccounts_ShouldDeleteUser()
    {
        // Arrange
        var userId = "usr-123";
        var user = new User { Id = userId };

        _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _unitOfWorkMock.Setup(u => u.BankAccounts.GetByUserIdAsync(userId))
            .ReturnsAsync(new List<BankAccount>());
        _unitOfWorkMock.Setup(u => u.CompleteAsync())
            .ReturnsAsync(1);

        // Act
        await _userService.DeleteUserAsync(userId, userId);

        // Assert
        _unitOfWorkMock.Verify(u => u.Users.Remove(user), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_WhenUserHasAccounts_ShouldThrowException()
    {
        // Arrange
        var userId = "usr-123";
        var user = new User { Id = userId };
        var accounts = new List<BankAccount>
        {
            new BankAccount { AccountNumber = "01234567", UserId = userId }
        };

        _unitOfWorkMock.Setup(u => u.Users.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _unitOfWorkMock.Setup(u => u.BankAccounts.GetByUserIdAsync(userId))
            .ReturnsAsync(accounts);

        // Act
        Func<Task> act = async () => await _userService.DeleteUserAsync(userId, userId);

        // Assert
        await act.Should().ThrowAsync<UserHasAccountsException>()
            .WithMessage("Cannot delete user 'usr-123' because they have 1 active bank account(s)");
    }
}
