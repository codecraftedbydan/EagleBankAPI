using EagleBankAPI.Core.Entities;
using EagleBankAPI.Core.Repositories;
using EagleBankAPI.DAL.Data;
using Microsoft.EntityFrameworkCore;
namespace EagleBankAPI.DAL.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(EagleBankDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email);
    }
}
