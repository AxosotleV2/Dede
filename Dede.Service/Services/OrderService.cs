using Dede.Domain.Entities;
using Dede.Domain.Interfaces;

namespace Dede.Service.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IServiceItemRepository _serviceRepository;

    public OrderService(IOrderRepository orderRepository,
                        IServiceItemRepository serviceRepository)
    {
        _orderRepository = orderRepository;
        _serviceRepository = serviceRepository;
    }

    public async Task<Order> CreateOrderAsync(int userId, int serviceItemId, int quantity,
        string phone, string address, string? note)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceItemId)
                     ?? throw new InvalidOperationException("Услуга не найдена");

        var order = new Order
        {
            UserId = userId,
            Phone = phone,
            Address = address,
            Note = note ?? string.Empty,
            Status = OrderStatus.New,
            CreatedAt = DateTime.UtcNow,
            Items =
            {
                new OrderItem
                {
                    ServiceItemId = serviceItemId,
                    Quantity = quantity
                }
            }
        };

        return await _orderRepository.AddAsync(order);
    }

    public Task<List<Order>> GetUserOrdersAsync(int userId)
        => _orderRepository.GetByUserAsync(userId);

    public async Task CancelOrderAsync(int userId, int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId)
                    ?? throw new InvalidOperationException("Заказ не найден");

        if (order.UserId != userId)
            throw new UnauthorizedAccessException("Нельзя отменить чужой заказ");

        if (order.Status == OrderStatus.Completed || order.Status == OrderStatus.Cancelled)
            return;

        order.Status = OrderStatus.Cancelled;
        await _orderRepository.UpdateAsync(order);
    }
}
