using EagleBankAPI.Core.Entities;
using EagleBankAPI.Core.Exceptions;
using EagleBankAPI.Core.Repositories;
using EagleBankAPI.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace EagleBankAPI.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _configurationMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        var jwtSettingsMock = new Mock<IConfigurationSection>();
        jwtSettingsMock.Setup(x => x["SecretKey"]).Returns("YourSuperSecretKeyForEagleBankAPI2026MinimumLength32Characters!");
        jwtSettingsMock.Setup(x => x["Issuer"]).Returns("EagleBankAPI");
        jwtSettingsMock.Setup(x => x["Audience"]).Returns("EagleBankAPIUsers");
        jwtSettingsMock.Setup(x => x["ExpiryMinutes"]).Returns("60");

        _configurationMock.Setup(x => x.GetSection("JwtSettings")).Returns(jwtSettingsMock.Object);

        _authService = new AuthService(_unitOfWorkMock.Object, _configurationMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var password = "SecurePassword123!";
        var hashedPassword = _authService.HashPassword(password);

        var user = new User
        {
            Id = "usr-123",
            Name = "John Doe",
            Email = "john@example.com",
            PasswordHash = hashedPassword
        };

        _unitOfWorkMock.Setup(u => u.Users.GetByEmailAsync(user.Email))
            .ReturnsAsync(user);

        // Act
        var (returnedUser, token, expiresAt) = await _authService.LoginAsync(user.Email, password);

        // Assert
        returnedUser.Should().NotBeNull();
        returnedUser.Id.Should().Be(user.Id);
        token.Should().NotBeNullOrEmpty();
        expiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ShouldThrowException()
    {
        // Arrange
        var email = "nonexistent@example.com";
        var password = "password";

        _unitOfWorkMock.Setup(u => u.Users.GetByEmailAsync(email))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _authService.LoginAsync(email, password);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowException()
    {
        // Arrange
        var correctPassword = "CorrectPassword123!";
        var hashedPassword = _authService.HashPassword(correctPassword);
        var email = "john@example.com";
        var wrongPassword = "WrongPassword123!";

        var user = new User
        {
            Id = "usr-123",
            Email = email,
            PasswordHash = hashedPassword
        };

        _unitOfWorkMock.Setup(u => u.Users.GetByEmailAsync(email))
            .ReturnsAsync(user);

        // Act
        Func<Task> act = async () => await _authService.LoginAsync(email, wrongPassword);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("Invalid email or password");
    }

    [Fact]
    public void HashPassword_ShouldGenerateHashedPassword()
    {
        // Arrange
        var password = "MySecurePassword123!";

        // Act
        var hashedPassword = _authService.HashPassword(password);

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().NotBe(password);
        hashedPassword.Length.Should().BeGreaterThan(40);
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "MySecurePassword123!";
        var hashedPassword = _authService.HashPassword(password);

        // Act
        var result = _authService.VerifyPassword(password, hashedPassword);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var correctPassword = "CorrectPassword123!";
        var wrongPassword = "WrongPassword123!";
        var hashedPassword = _authService.HashPassword(correctPassword);

        // Act
        var result = _authService.VerifyPassword(wrongPassword, hashedPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GenerateJwtToken_ShouldCreateValidToken()
    {
        // Arrange
        var userId = "usr-123";
        var name = "John Doe";
        var email = "john@example.com";

        // Act
        var token = _authService.GenerateJwtToken(userId, name, email);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3);
    }

    [Fact]
    public void HashPassword_WithSamePassword_ShouldGenerateDifferentHashes()
    {
        // Arrange
        var password = "SamePassword123!";

        // Act
        var hash1 = _authService.HashPassword(password);
        var hash2 = _authService.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2);
        _authService.VerifyPassword(password, hash1).Should().BeTrue();
        _authService.VerifyPassword(password, hash2).Should().BeTrue();
    }
}
