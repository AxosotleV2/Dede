using Dede.Domain.Entities;

namespace Dede.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order> AddAsync(Order order);
    Task<Order?> GetByIdAsync(int id);

    Task<List<Order>> GetByUserAsync(int userId);

    Task UpdateAsync(Order order);
}