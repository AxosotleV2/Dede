// Dede.DAL/Repositories/UserRepository.cs

using Dede.Domain.Entities;
using Dede.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dede.DAL.Repositories;

public class UserRepository(DedeDbContext ctx) : IUserRepository
{
    public Task<User?> GetByIdAsync(int id)
    {
        return ctx.Users.FindAsync(id).AsTask();
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        return ctx.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public Task<bool> EmailExistsAsync(string email)
    {
        return ctx.Users.AnyAsync(u => u.Email == email);
    }

    public async Task AddAsync(User user)
    {
        await ctx.Users.AddAsync(user);
    }

    public Task SaveChangesAsync() => ctx.SaveChangesAsync();
    
}