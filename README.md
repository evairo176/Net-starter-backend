# Net Starter Backend

Starter ASP.NET Core Web API (.NET 10) - langsung jalan dengan **Swagger UI**, **EF Core + SQL Server**, **envelope response standar**, **Docker Compose**, dan contoh CRUD `Product`.

## Struktur (pola project-management)

```
NetStarter.Api/
├── Controllers/ProductsController.cs   # Controller tipis, mapping ServiceResult -> envelope
├── Dtos/
│   ├── Api/ApiResult.cs                # ApiResult.Ok / Created / Paged / Error / NotFound / Conflict
│   └── Product/ProductDtos.cs          # Request & response records
├── Entities/Product.cs                 # Entity (snake_case di DB)
├── Data/AppDbContext.cs                # EF Core + mapping snake_case
├── Helpers/JakartaTime.cs              # Waktu WIB (UTC+7)
├── Migrations/                         # InitialCreate (auto-apply di startup)
├── Services/
│   ├── ServiceResult.cs                # ServiceResult<T> (Success/NotFound/Conflict/...)
│   ├── Interfaces/Product/IProductService.cs
│   └── Implementations/Product/ProductService.cs
├── Program.cs                          # DI, Swagger, CORS, Health, Migrate + Seed
├── appsettings.json
└── appsettings.Development.json
```

## Cara jalan (cepat)

### 1. Pakai Docker Compose (SQL Server + API)

```bash
docker compose up -d --build
# API: http://localhost:5028   Swagger: http://localhost:5028/swagger
```

### 2. Local dev (butuh SQL Server)

```bash
# 1) SQL Server (kalau belum ada)
docker run -d --name sqlserver -e ACCEPT_EULA=Y -e 'MSSQL_SA_PASSWORD=YourStrong@Passw0rd' \
  -e MSSQL_PID=Express -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest

# 2) Jalanin API
cd NetStarter.Api
dotnet restore
dotnet run
# Swagger: http://localhost:5036/swagger
```

Database di-migrate + di-seed otomatis saat startup (5 product contoh). Kalau DB belum nyala, app tetap jalan - health + swagger hidup, log warning kasih tau cara nyalain DB.

### 2b. Windows (LocalDB - tanpa install SQL Server penuh)

Kalau lo pakai Windows dan gagal konek `localhost:1433` (SQL Server gak terpasang):

```powershell
# Cek LocalDB udah ada?
sqllocaldb info          # harus ada MSSQLLocalDB

# Kalau ada - langsung jalan (appsettings.Development.json udah pake LocalDB)
dotnet run

# Kalau belum ada - install LocalDB (via Visual Studio Installer / BDL):
#   https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb
# atau pakai Docker Desktop:
docker compose up -d
```

## Endpoint

| Method | Path | Keterangan |
|---|---|---|
| GET | `/api/products?page=1&limit=10&search=` | List product (paginasi + search) |
| GET | `/api/products/{id}` | Detail |
| POST | `/api/products` | Buat baru |
| PUT | `/api/products/{id}` | Update |
| DELETE | `/api/products/{id}` | Hapus |
| GET | `/health` | Health check |

## Envelope response

Semua endpoint pakai satu shape (via `ApiResult`):

```json
{
  "success": true,
  "message": "Success",
  "data": [],
  "meta": {
    "page": 1,
    "limit": 10,
    "totalItems": 5,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  }
}
```

Error: `success: false` + `code`: `NOT_FOUND` / `VALIDATION_ERROR` / `CONFLICT` / `FORBIDDEN` / `INTERNAL_ERROR`.

Flow: Controller → Service mengembalikan `ServiceResult<T>` (Success/NotFound/Conflict/Forbidden/BadRequest/Fail) → Controller map ke `ApiResult.*` + status code. Layer service ga bolak-balik HTTP.

## Migrations

```bash
cd NetStarter.Api
dotnet tool restore
dotnet tool run dotnet-ef migrations add NamaPerubahan   # bikin migration baru
dotnet tool run dotnet-ef database update                 # apply manual (biasanya auto di startup)
```

## Konfigurasi

- Connection string: `appsettings.json` → `ConnectionStrings:Default` (default: `localhost,1433` / SA).
- Production: set env `ConnectionStrings__Default=...` (lihat `docker-compose.yml`).
- CORS: `Cors:Origins` (default dev: `*`).
- Port local: `dotnet run --urls http://localhost:5099`.