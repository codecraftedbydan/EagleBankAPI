using EagleBankAPI.Core.Entities;
using EagleBankAPI.Core.Exceptions;
using EagleBankAPI.Core.Repositories;
using EagleBankAPI.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace EagleBankAPI.Core.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthService _authService;
    private readonly ILogger<UserService> _logger;

    public UserService(IUnitOfWork unitOfWork, IAuthService authService, ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _authService = authService;
        _logger = logger;
    }

    public async Task<User> CreateUserAsync(string name, string email, string password, string phoneNumber,
        string addressLine1, string? addressLine2, string? addressLine3, string town, string county, string postcode)
    {
        _logger.LogInformation("Creating new user with email: {Email}", email);

        // Check if email already exists
        if (await _unitOfWork.Users.EmailExistsAsync(email))
        {
            _logger.LogWarning("User creation failed - duplicate email: {Email}", email);
            throw new DuplicateEmailException(email);
        }

        var user = new User
        {
            Id = $"usr-{Guid.NewGuid():N}",
            Name = name,
            Email = email,
            PasswordHash = _authService.HashPassword(password),
            PhoneNumber = phoneNumber,
            AddressLine1 = addressLine1,
            AddressLine2 = addressLine2,
            AddressLine3 = addressLine3,
            AddressTown = town,
            AddressCounty = county,
            AddressPostcode = postcode,
            CreatedTimestamp = DateTime.UtcNow,
            UpdatedTimestamp = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("User created successfully with ID: {UserId}, Email: {Email}", user.Id, email);
        return user;
    }

    public async Task<User?> GetUserByIdAsync(string userId, string requestingUserId)
    {
        _logger.LogInformation("User {RequestingUserId} requesting details for user {UserId}", requestingUserId, userId);

        // Users can only view their own details
        if (userId != requestingUserId)
        {
            _logger.LogWarning("Forbidden: User {RequestingUserId} attempted to access user {UserId}", requestingUserId, userId);
            throw new ForbiddenException("You can only view your own user details");
        }

        return await _unitOfWork.Users.GetByIdAsync(userId);
    }

    public async Task<User> UpdateUserAsync(string userId, string? name, string? email, string? phoneNumber,
        string? addressLine1, string? addressLine2, string? addressLine3, string? town, string? county, string? postcode, string requestingUserId)
    {
        // Users can only update their own details
        if (userId != requestingUserId)
        {
            throw new ForbiddenException("You can only update your own user details");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        if (name != null) user.Name = name;

        if (email != null)
        {
            if (email != user.Email && await _unitOfWork.Users.EmailExistsAsync(email))
            {
                throw new DuplicateEmailException(email);
            }
            user.Email = email;
        }

        if (phoneNumber != null) user.PhoneNumber = phoneNumber;

        if (addressLine1 != null)
        {
            user.AddressLine1 = addressLine1;
            user.AddressLine2 = addressLine2;
            user.AddressLine3 = addressLine3;
            user.AddressTown = town!;
            user.AddressCounty = county!;
            user.AddressPostcode = postcode!;
        }
        user.UpdatedTimestamp = DateTime.UtcNow;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("User updated successfully: {UserId}", userId);
        return user;
    }


    public async Task DeleteUserAsync(string userId, string requestingUserId)
    {
        // Users can only delete their own account
        if (userId != requestingUserId)
        {
            throw new ForbiddenException("You can only delete your own user account");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        // Check if user has any bank accounts
        var accounts = await _unitOfWork.BankAccounts.GetByUserIdAsync(userId);
        if (accounts.Any())
        {
            throw new UserHasAccountsException(userId, accounts.Count());
        }

        _unitOfWork.Users.Remove(user);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("User deleted successfully: {UserId}", userId);
    }
}
