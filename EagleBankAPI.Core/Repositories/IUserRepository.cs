using EagleBankAPI.Core.Entities;

namespace EagleBankAPI.Core.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
}
