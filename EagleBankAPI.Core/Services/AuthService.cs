using EagleBankAPI.Core.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using EagleBankAPI.Core.Repositories;
using EagleBankAPI.Core.Entities;
using EagleBankAPI.Core.Services.Interfaces;

namespace EagleBankAPI.Core.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(User user, string token, DateTime expiresAt)> LoginAsync(string email, string password)
    {
        _logger.LogInformation("Login attempt for email: {Email}", email);
        
        var user = await _unitOfWork.Users.GetByEmailAsync(email);
        if (user == null)
        {
            _logger.LogWarning("Login failed - user not found: {Email}", email);
            throw new ForbiddenException("Invalid email or password");
        }

        if (!VerifyPassword(password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed - invalid password for email: {Email}", email);
            throw new ForbiddenException("Invalid email or password");
        }

        var token = GenerateJwtToken(user.Id, user.Name, user.Email);
        var expiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration.GetSection("JwtSettings")["ExpiryMinutes"] ?? "60"));

        _logger.LogInformation("Login successful for user: {UserId}, Email: {Email}", user.Id, email);
        return (user, token, expiresAt);
    }

    public string GenerateJwtToken(string userId, string name, string email)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyForEagleBankAPI2026MinimumLength32Characters!";
        var issuer = jwtSettings["Issuer"] ?? "EagleBankAPI";
        var audience = jwtSettings["Audience"] ?? "EagleBankAPIUsers";
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Name, name),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("userId", userId)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string HashPassword(string password)
    {
        // Using PBKDF2 with HMACSHA256
        byte[] salt = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 10000, HashAlgorithmName.SHA256, 32);

        // Combine salt and hash
        byte[] hashBytes = new byte[48];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 32);

        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        try
        {
            byte[] hashBytes = Convert.FromBase64String(passwordHash);

            // Extract salt
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            // Compute hash with extracted salt
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 10000, HashAlgorithmName.SHA256, 32);

            // Compare hashes
            for (int i = 0; i < 32; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}
