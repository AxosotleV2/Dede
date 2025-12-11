namespace Dede.Domain.Entities;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ServiceItemId { get; set; }
    public ServiceItem ServiceItem { get; set; } = null!;

    public int Quantity { get; set; } = 1;
    public decimal Price { get; set; } // цена за позицию (MinPrice * Quantity)
}