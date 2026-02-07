using Microsoft.EntityFrameworkCore;
using QAWebApp.Data;
using QAWebApp.Models;
using QAWebApp.Repositories.Interfaces;

namespace QAWebApp.Repositories.Implementations;

public class UserRepository : Repository<ApplicationUser>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ApplicationUser?> GetByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _dbSet.AnyAsync(u => u.Username == username);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email);
    }

    public async Task UpdateReputationAsync(int userId, int reputationChange)
    {
        var user = await _dbSet.FindAsync(userId);
        if (user != null)
        {
            user.Reputation += reputationChange;
        }
    }
}