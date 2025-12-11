using Dede.Domain.Entities;

namespace Dede.Domain.Interfaces;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(int userId, int serviceItemId, int quantity,
        string phone, string address, string? note);

    Task<List<Order>> GetUserOrdersAsync(int userId);

    Task CancelOrderAsync(int userId, int orderId);
    
}