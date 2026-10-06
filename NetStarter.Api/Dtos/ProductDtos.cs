using System.ComponentModel.DataAnnotations;

namespace NetStarter.Api.Dtos;

// Request
public record CreateProductRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(50)] string? Category,
    [Range(0, double.MaxValue)] decimal Price,
    [Range(0, int.MaxValue)] int Stock);

public record UpdateProductRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(50)] string? Category,
    [Range(0, double.MaxValue)] decimal Price,
    [Range(0, int.MaxValue)] int Stock);

public record ProductQueryRequest(int Page = 1, int Limit = 10, string? Search = null);

// Response
public record ProductDto(Guid Id, string Name, string? Category, decimal Price, int Stock, DateTime CreatedAt, DateTime? UpdatedAt);

public record PagedMeta(int CurrentPage, int PerPage, long Total, int TotalPages);