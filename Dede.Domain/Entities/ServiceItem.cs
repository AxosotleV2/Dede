namespace Dede.Domain.Entities;

public class ServiceItem
{
    public int Id { get; set; }
    public string Name { get; set; } = null!; // "Сантехника"
    public string Description { get; set; } = null!;
    public decimal MinPrice { get; set; } // 800, 1000...
    public string Category { get; set; } = null!; // можно тоже "Сантехника"
    public string Icon { get; set; } = "🔧"; // эмодзи или css-иконка
    public bool IsActive { get; set; } = true;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}