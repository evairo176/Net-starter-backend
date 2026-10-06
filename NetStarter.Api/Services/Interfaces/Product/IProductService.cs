using NetStarter.Api.Dtos.Product;

namespace NetStarter.Api.Services.Interfaces.Product;

public interface IProductService
{
    Task<ServiceResult<PagedProductsDto>> GetPagedAsync(ProductQueryRequest query, CancellationToken ct);
    Task<ServiceResult<ProductDto>> GetByIdAsync(Guid id, CancellationToken ct);
    Task<ServiceResult<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken ct);
    Task<ServiceResult<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct);
    Task<ServiceResult<Guid>> DeleteAsync(Guid id, CancellationToken ct);
    Task SeedAsync(CancellationToken ct);
}