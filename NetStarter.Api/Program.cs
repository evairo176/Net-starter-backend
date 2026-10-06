using Microsoft.EntityFrameworkCore;
using NetStarter.Api.Data;
using NetStarter.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// === Services (DI) ===
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "NetStarter API",
        Version = "v1",
        Description = "Starter ASP.NET Core Web API - envelope response, EF Core SQL Server, Swagger UI.",
    });
});

// Database: SQL Server (ganti connection string di appsettings / env).
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IProductService, ProductService>();

// CORS (dev: boleh semua origin; produksi: kunci ke origin FE).
var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["*"];
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyHeader().AllowAnyMethod()
        .WithOrigins(corsOrigins)
        .SetIsOriginAllowedToAllowWildcardSubdomains()));

builder.Services.AddHealthChecks();

var app = builder.Build();

// === Database migrate + seed (toleran offline biar swagger/health tetap jalan) ===
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<IProductService>().SeedAsync(default);
    app.Logger.LogInformation("Database ready & seeded.");
}
catch (Exception ex)
{
    app.Logger.LogWarning("Database belum bisa diakses: {Message} (jalankan docker compose up -d sqlserver)", ex.Message);
}

// === Middleware ===
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Swagger tetap bisa di-enable di staging bareng appsettings Production
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;