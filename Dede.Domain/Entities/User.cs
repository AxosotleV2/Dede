namespace Dede.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Role { get; set; } = "User"; // User / Admin
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool EmailConfirmed { get; set; }
    public string? EmailConfirmationToken { get; set; }
    public DateTime? EmailConfirmationTokenExpiresAt { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}