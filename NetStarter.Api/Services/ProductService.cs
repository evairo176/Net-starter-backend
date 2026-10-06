using Microsoft.EntityFrameworkCore;
using NetStarter.Api.Data;
using NetStarter.Api.Dtos;
using NetStarter.Api.Entities;

namespace NetStarter.Api.Services;

public interface IProductService
{
    Task<(List<ProductDto> Items, long Total)> GetPagedAsync(ProductQueryRequest query, CancellationToken ct);
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct);
    Task<ProductDto?> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct);
    Task<Guid?> DeleteAsync(Guid id, CancellationToken ct);
    Task SeedAsync(CancellationToken ct);
}

public class ProductService(AppDbContext db) : IProductService
{
    public async Task<(List<ProductDto> Items, long Total)> GetPagedAsync(ProductQueryRequest query, CancellationToken ct)
    {
        var q = db.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim().ToLower();
            q = q.Where(p => p.Name.ToLower().Contains(s) || (p.Category != null && p.Category.ToLower().Contains(s)));
        }

        var total = await q.LongCountAsync(ct);
        var items = await q
            .OrderBy(p => p.Name)
            .Skip((query.Page - 1) * query.Limit)
            .Take(query.Limit)
            .Select(p => ToDto(p))
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var p = await db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return p is null ? null : ToDto(p);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct)
    {
        var product = new Product
        {
            Name = request.Name.Trim(),
            Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim(),
            Price = request.Price,
            Stock = request.Stock,
            CreatedAt = DateTime.UtcNow,
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
        return ToDto(product);
    }

    public async Task<ProductDto?> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct)
    {
        var p = await db.Products.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return null;

        p.Name = request.Name.Trim();
        p.Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim();
        p.Price = request.Price;
        p.Stock = request.Stock;
        p.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return ToDto(p);
    }

    public async Task<Guid?> DeleteAsync(Guid id, CancellationToken ct)
    {
        var p = await db.Products.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return null;

        db.Products.Remove(p);
        await db.SaveChangesAsync(ct);
        return id;
    }

    public async Task SeedAsync(CancellationToken ct)
    {
        if (await db.Products.AnyAsync(ct)) return;

        db.Products.AddRange(
            new Product { Name = "Keyboard Mechanical", Category = "Accessories", Price = 450000, Stock = 25, CreatedAt = DateTime.UtcNow },
            new Product { Name = "Monitor 24 inch", Category = "Display", Price = 1850000, Stock = 12, CreatedAt = DateTime.UtcNow },
            new Product { Name = "Mouse Wireless", Category = "Accessories", Price = 125000, Stock = 40, CreatedAt = DateTime.UtcNow },
            new Product { Name = "Laptop Stand", Category = "Ergonomics", Price = 150000, Stock = 18, CreatedAt = DateTime.UtcNow },
            new Product { Name = "USB-C Dock", Category = "Accessories", Price = 750000, Stock = 8, CreatedAt = DateTime.UtcNow });

        await db.SaveChangesAsync(ct);
    }

    private static ProductDto ToDto(Product p) =>
        new(p.Id, p.Name, p.Category, p.Price, p.Stock, p.CreatedAt, p.UpdatedAt);
}