using Microsoft.AspNetCore.Mvc;
using NetStarter.Api.Dtos.Api;
using NetStarter.Api.Dtos.Product;
using NetStarter.Api.Services.Interfaces.Product;

namespace NetStarter.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(IProductService products) : ControllerBase
{
    /// <summary>List product (paginasi + search).</summary>
    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string? search = null, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        var result = await products.GetPagedAsync(new ProductQueryRequest(page, limit, search), ct);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, ApiResult.Error(result.Error!, "INTERNAL_ERROR"));

        var paged = result.Data!;
        return Ok(ApiResult.Paged(paged.Items, paged.Page, paged.Limit, paged.Total));
    }

    /// <summary>Detail product by id.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await products.GetByIdAsync(id, ct);
        return result.IsSuccess
            ? Ok(ApiResult.Ok(result.Data!))
            : StatusCode(result.StatusCode, ApiResult.NotFound(result.Error!));
    }

    /// <summary>Buat product baru.</summary>
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResult.BadRequest("Validasi gagal"));

        var result = await products.CreateAsync(request, ct);
        return result.IsSuccess
            ? Ok(ApiResult.Created(result.Data!))
            : StatusCode(result.StatusCode, ApiResult.Error(result.Error!, ErrorCodeFor(result.StatusCode)));
    }

    /// <summary>Update product.</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResult.BadRequest("Validasi gagal"));

        var result = await products.UpdateAsync(id, request, ct);
        return result.IsSuccess
            ? Ok(ApiResult.Ok(result.Data!, "Product updated"))
            : StatusCode(result.StatusCode, ApiResult.Error(result.Error!, ErrorCodeFor(result.StatusCode)));
    }

    /// <summary>Hapus product.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await products.DeleteAsync(id, ct);
        return result.IsSuccess
            ? Ok(ApiResult.Ok(result.Data, "Product deleted"))
            : StatusCode(result.StatusCode, ApiResult.NotFound(result.Error!));
    }

    private static string ErrorCodeFor(int status) => status switch
    {
        400 => "VALIDATION_ERROR",
        401 => "UNAUTHORIZED",
        403 => "FORBIDDEN",
        404 => "NOT_FOUND",
        409 => "CONFLICT",
        _ => "INTERNAL_ERROR",
    };
}