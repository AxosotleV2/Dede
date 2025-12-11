using Dede.Domain.Entities;
using Dede.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dede.DAL.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly DedeDbContext _context;

    public OrderRepository(DedeDbContext context)
    {
        _context = context;
    }

    public async Task<Order> AddAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.ServiceItem)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<Order>> GetByUserAsync(int userId)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.ServiceItem)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }
}