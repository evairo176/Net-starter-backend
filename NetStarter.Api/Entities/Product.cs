using System.ComponentModel.DataAnnotations;

namespace NetStarter.Api.Entities;

/// <summary>
/// Contoh entity CRUD. Ganti / tambah entity sesuai kebutuhan project.
/// Konvensi: snake_case di database (via HasColumnName di AppDbContext).
/// </summary>
public class Product
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Category { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}