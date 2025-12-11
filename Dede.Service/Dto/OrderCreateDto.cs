namespace Dede.Service.Dto;

public class OrderCreateDto
{
    public int ServiceItemId { get; set; }
    public int Quantity { get; set; } = 1;
    public string Phone { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string Note { get; set; } = string.Empty;
}