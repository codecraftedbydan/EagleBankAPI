using EagleBankAPI.Core.Entities;

namespace EagleBankAPI.Core.Services.Interfaces;

public interface IUserService
{
    Task<User> CreateUserAsync(string name, string email, string password, string phoneNumber, 
        string addressLine1, string? addressLine2, string? addressLine3, string town, string county, string postcode);
    Task<User?> GetUserByIdAsync(string userId, string requestingUserId);
    Task<User> UpdateUserAsync(string userId, string name, string email, string phoneNumber,
        string addressLine1, string? addressLine2, string? addressLine3, string town, string county, string postcode, string requestingUserId);
    Task DeleteUserAsync(string userId, string requestingUserId);
}
