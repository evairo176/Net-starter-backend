using Microsoft.EntityFrameworkCore;
using NetStarter.Api.Data;
using NetStarter.Api.Dtos.Product;
using NetStarter.Api.Entities;
using NetStarter.Api.Helpers;
using NetStarter.Api.Services.Interfaces.Product;

namespace NetStarter.Api.Services.Implementations.Product;

public class ProductService(AppDbContext db) : IProductService
{
    public async Task<ServiceResult<PagedProductsDto>> GetPagedAsync(ProductQueryRequest query, CancellationToken ct)
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

        return ServiceResult<PagedProductsDto>.Success(new PagedProductsDto(items, total, query.Page, query.Limit));
    }

    public async Task<ServiceResult<ProductDto>> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var p = await db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return p is null
            ? ServiceResult<ProductDto>.NotFound("Product tidak ditemukan.")
            : ServiceResult<ProductDto>.Success(ToDto(p));
    }

    public async Task<ServiceResult<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken ct)
    {
        var duplicate = await db.Products.AnyAsync(p => p.Name == request.Name.Trim(), ct);
        if (duplicate)
            return ServiceResult<ProductDto>.Conflict("Nama product sudah dipakai.");

        var product = new Entities.Product
        {
            Name = request.Name.Trim(),
            Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim(),
            Price = request.Price,
            Stock = request.Stock,
            CreatedAt = JakartaTime.Now,
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
        return ServiceResult<ProductDto>.Success(ToDto(product));
    }

    public async Task<ServiceResult<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct)
    {
        var p = await db.Products.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null)
            return ServiceResult<ProductDto>.NotFound("Product tidak ditemukan.");

        var duplicate = await db.Products.AnyAsync(x => x.Name == request.Name.Trim() && x.Id != id, ct);
        if (duplicate)
            return ServiceResult<ProductDto>.Conflict("Nama product sudah dipakai.");

        p.Name = request.Name.Trim();
        p.Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim();
        p.Price = request.Price;
        p.Stock = request.Stock;
        p.UpdatedAt = JakartaTime.Now;

        await db.SaveChangesAsync(ct);
        return ServiceResult<ProductDto>.Success(ToDto(p));
    }

    public async Task<ServiceResult<Guid>> DeleteAsync(Guid id, CancellationToken ct)
    {
        var p = await db.Products.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null)
            return ServiceResult<Guid>.NotFound("Product tidak ditemukan.");

        db.Products.Remove(p);
        await db.SaveChangesAsync(ct);
        return ServiceResult<Guid>.Success(id);
    }

    public async Task SeedAsync(CancellationToken ct)
    {
        if (await db.Products.AnyAsync(ct)) return;

        db.Products.AddRange(
            new Entities.Product { Name = "Keyboard Mechanical", Category = "Accessories", Price = 450000, Stock = 25, CreatedAt = JakartaTime.Now },
            new Entities.Product { Name = "Monitor 24 inch", Category = "Display", Price = 1850000, Stock = 12, CreatedAt = JakartaTime.Now },
            new Entities.Product { Name = "Mouse Wireless", Category = "Accessories", Price = 125000, Stock = 40, CreatedAt = JakartaTime.Now },
            new Entities.Product { Name = "Laptop Stand", Category = "Ergonomics", Price = 150000, Stock = 18, CreatedAt = JakartaTime.Now },
            new Entities.Product { Name = "USB-C Dock", Category = "Accessories", Price = 750000, Stock = 8, CreatedAt = JakartaTime.Now });

        await db.SaveChangesAsync(ct);
    }

    private static ProductDto ToDto(Entities.Product p) =>
        new(p.Id, p.Name, p.Category, p.Price, p.Stock, p.CreatedAt, p.UpdatedAt);
}