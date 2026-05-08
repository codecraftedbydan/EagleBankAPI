using EagleBankAPI.Core.Entities;

namespace EagleBankAPI.Core.Services.Interfaces;

public interface IAuthService
{
    Task<(User user, string token, DateTime expiresAt)> LoginAsync(string email, string password);
    string GenerateJwtToken(string userId, string name, string email);
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}
