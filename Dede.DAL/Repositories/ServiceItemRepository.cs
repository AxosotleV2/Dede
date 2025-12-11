// Dede.DAL/Repositories/ServiceItemRepository.cs

using Dede.Domain.Entities;
using Dede.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dede.DAL.Repositories;

public class ServiceItemRepository : IServiceItemRepository
{
    private readonly DedeDbContext _ctx;

    public ServiceItemRepository(DedeDbContext ctx)
    {
        _ctx = ctx;
    }

    public Task<ServiceItem?> GetByIdAsync(int id)
    {
        return _ctx.Services.FindAsync(id).AsTask();
    }

    public async Task<List<ServiceItem>> GetAllAsync(string? sortByPrice = null)
    {
        IQueryable<ServiceItem> query = _ctx.Services;

        if (sortByPrice == "priceAsc")
            query = query.OrderBy(s => s.MinPrice);
        else if (sortByPrice == "priceDesc")
            query = query.OrderByDescending(s => s.MinPrice);

        return await query.ToListAsync();
    }

    public async Task AddAsync(ServiceItem service)
    {
        await _ctx.Services.AddAsync(service);
    }

    public Task UpdateAsync(ServiceItem service)
    {
        _ctx.Services.Update(service);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ServiceItem service)
    {
        _ctx.Services.Remove(service);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        return _ctx.SaveChangesAsync();
    }
}