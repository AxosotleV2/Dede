namespace Dede.Domain.Entities;

public enum OrderStatus
{
    Draft = 0,
    New = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Phone { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string Note { get; set; } = string.Empty;

    public OrderStatus Status { get; set; } = OrderStatus.New;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}