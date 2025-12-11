using System.ComponentModel.DataAnnotations;

namespace Dede.Models;

public class ServiceEditModel
{
    [Required] [MaxLength(200)] public string Name { get; set; } = "";

    [Required] public string Description { get; set; } = "";

    [Required] [Range(0, 1_000_000)] public decimal MinPrice { get; set; }

    [Required] [MaxLength(100)] public string Category { get; set; } = "";

    [Required] [MaxLength(50)] public string Icon { get; set; } = "🔧";

    public bool IsActive { get; set; } = true;
}