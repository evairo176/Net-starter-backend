using Microsoft.AspNetCore.Mvc;
using NetStarter.Api.Common;
using NetStarter.Api.Dtos;
using NetStarter.Api.Services;

namespace NetStarter.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(IProductService products) : ControllerBase
{
    /// <summary>List product (paginasi + search).</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> GetAll([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string? search = null, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        var (items, total) = await products.GetPagedAsync(new ProductQueryRequest(page, limit, search), ct);
        var totalPages = (int)Math.Ceiling((double)total / limit);

        return Ok(new
        {
            success = true,
            message = "Data retrieved successfully",
            data = items,
            meta = new
            {
                pagination = new
                {
                    current_page = page,
                    per_page = limit,
                    total,
                    total_pages = totalPages,
                    from = items.Count == 0 ? 0 : (page - 1) * limit + 1,
                    to = (page - 1) * limit + items.Count,
                },
            },
            error = (object?)null,
        });
    }

    /// <summary>Detail product by id.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(Guid id, CancellationToken ct)
    {
        var product = await products.GetByIdAsync(id, ct);
        return product is null
            ? NotFound(ApiResponse<ProductDto>.Fail("Product tidak ditemukan", ErrorCodes.NotFound))
            : Ok(ApiResponse<ProductDto>.Ok(product));
    }

    /// <summary>Buat product baru.</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<ProductDto>.Fail("Validation failed", ErrorCodes.Validation, ModelState));

        var product = await products.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, ApiResponse<ProductDto>.Ok(product, "Product created"));
    }

    /// <summary>Update product.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<ProductDto>.Fail("Validation failed", ErrorCodes.Validation, ModelState));

        var product = await products.UpdateAsync(id, request, ct);
        return product is null
            ? NotFound(ApiResponse<ProductDto>.Fail("Product tidak ditemukan", ErrorCodes.NotFound))
            : Ok(ApiResponse<ProductDto>.Ok(product, "Product updated"));
    }

    /// <summary>Hapus product.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<Guid>>> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await products.DeleteAsync(id, ct);
        return deleted is null
            ? NotFound(ApiResponse<Guid>.Fail("Product tidak ditemukan", ErrorCodes.NotFound))
            : Ok(ApiResponse<Guid>.Ok(deleted.Value, "Product deleted"));
    }
}