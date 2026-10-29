# ClothesStore backend

A .NET 10 / ASP.NET Core API using PostgreSQL, EF Core, Identity, and JWT. Open `ClothesStore.slnx` in Rider.

## Implemented

- Customer registration, login, refresh-token rotation, logout, and User/Admin authorization.
- Public searchable, paginated catalog; admin category and brand CRUD, product creation/update/archive.
- Clothing variants with size, color, material, SKU, prices, active state, and stock.
- Product image upload/delete with local storage by default; optional Cloudinary.
- Persistent per-user carts with quantity and availability checks.
- Transactional checkout with server-calculated prices, stock deduction, and order-item snapshots.
- Customer order history and pending-order cancellation; admin fulfillment and cancellation.
- PostgreSQL migrations, OpenAPI/Swagger, Docker Compose, and PostgreSQL integration tests.

## Business rules

This version uses cash on delivery and free shipping. All prices use the store's single currency, configured operationally by the merchant; no currency conversion is performed. Discount is zero. There is no online payment provider, tax engine, coupon API, or returns/refund flow.

StockQuantity on each product variant is the source of truth. Existing Warehouse, Inventory, and Promotion domain types are retained for future work; their tables are not used by checkout and have no management endpoints. Do not independently populate Inventory expecting checkout to consume it.

Cart PUT sets the desired quantity (1–100), rather than adding to the existing quantity. Cart prices are current catalog prices; order prices are immutable snapshots. Products are archived instead of deleted so order history remains intact.

Allowed status transitions:
- Pending → Processing or Cancelled
- Processing → Shipped or Cancelled
- Shipped → Delivered
- Delivered and Cancelled are terminal.

Customers can only cancel their own Pending orders. Cancellation restores inventory exactly once. Checkout and cancellation run in serializable PostgreSQL transactions; concurrent conflicts return HTTP 409. Reload state before retrying. If a checkout response is lost, check order history before submitting again.

## Local setup (PowerShell)

Requirements: .NET **10 SDK** and PostgreSQL (tested with PostgreSQL 18). The .NET 10 runtime alone is insufficient.

On this computer, the SDK is installed at `C:\Users\minhn\.dotnet\dotnet.exe`. The system PATH currently selects an older SDK. In a project terminal:

```powershell
$env:DOTNET_ROOT = "$env:USERPROFILE\.dotnet"
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"
dotnet --version
dotnet restore ClothesStore.slnx
dotnet tool restore
```

Create a dedicated database and configure the application with environment variables or .NET user secrets. Existing `01.API/appsettings.json` has been preserved. Avoid putting new credentials in source control.

Example configuration (replace placeholders):

```powershell
$env:ConnectionStrings__DefaultConnection = 'Host=localhost;Port=5432;Database=clothesstore;Username=postgres;Password=YOUR_PASSWORD'
$env:Jwt__Key = 'YOUR_RANDOM_SECRET_OF_AT_LEAST_32_BYTES'
$env:Jwt__Issuer = 'ClothesStore'
$env:Jwt__Audience = 'ClothesStore'
$env:Jwt__ExpirationMinutes = '30'
$env:SeedAdmin__Email = 'admin@example.com'
$env:SeedAdmin__Password = 'YOUR_STRONG_ADMIN_PASSWORD'
dotnet ef database update --project '03. Infrastructure/03. Infrastructure.csproj' --startup-project '01.API/01. API.csproj'
dotnet run --project '01.API/01. API.csproj' --launch-profile http
```

Visit `http://localhost:5000/swagger`. OpenAPI JSON is at `/openapi/v1.json`; `/health` is a liveness endpoint. Swagger is enabled only in Development. Production requires HTTPS at the application or a correctly configured reverse proxy.

Roles are initialized at startup. SeedAdmin creates a **new** administrator account only; it never elevates an existing registered email. Remove the seed password from your environment after initial creation. Public registration always creates a User.

Migrations are not applied automatically by default. For local startup migration only, set `Database__ApplyMigrations=true`. Review and apply migrations separately when deploying an existing database.

## Docker Compose

With Docker running:

```powershell
Copy-Item .env.example .env
# Edit .env to set PostgreSQL, JWT, and optional admin credentials.
docker compose up --build
```

API/Swagger: `http://localhost:5080/swagger`. PostgreSQL is exposed locally on port 5433. Database and uploaded images use named volumes. Compose is a local-development configuration.

The Dockerfile was corrected to match the actual project filenames and references. The Docker image build was not executed during this implementation because Docker Desktop was not running.

## API usage

Use `01.API/01.API.http` in Rider, or Swagger.

1. Log in as the bootstrap administrator.
2. Create a brand and category.
3. Create a product with their IDs.
4. Add clothing variants and initial stock.
5. Register/log in as a customer; set cart items and submit an order.
6. Use the admin status endpoint to process, ship, and deliver orders.

Authentication responses preserve the original wrapper:
```json
{"status":200,"returnObject":{"accessToken":"...","refreshToken":"..."},"isSuccess":true}
```

Send the access token as `Authorization: Bearer <accessToken>`. New store endpoints return DTOs directly. Lists of products/orders return `{items,total,page,pageSize}`; deletion returns HTTP 204.

| Access | Endpoints |
| --- | --- |
| Public | POST /api/auth/register, /login, /refresh, /logout |
| Public | GET /api/categories, /api/brands, /api/products, /api/products/{id} |
| Admin | POST /api/categories, /api/brands, /api/products |
| Admin | PUT/DELETE /api/categories/{id}, /api/brands/{id}, /api/products/{id} |
| Admin | GET /api/admin/products (includes archived products and inactive variants) |
| Admin | POST /api/products/{id}/variants; PUT /api/products/{id}/variants/{variantId} |
| Admin | PUT /api/variants/{id}/stock |
| Admin | POST /api/products/{id}/images; DELETE /api/products/{id}/images/{imageId} |
| Customer | GET /api/cart; PUT /api/cart/items; DELETE /api/cart/items/{variantId} |
| Customer | POST/GET /api/orders; GET /api/orders/{id}; POST /api/orders/{id}/cancel |
| Admin | GET /api/admin/orders, /api/admin/orders/{id}; PUT /api/admin/orders/{id}/status |

Catalog query parameters: `search`, `categoryId`, `brandId`, `page` (default 1), `pageSize` (default 20, maximum 100). Category filtering matches that category, not its descendants.

Validation uses HTTP 400, missing/other-user resources use 404, authentication/authorization uses 401/403, and conflicting stock or transitions use 409. Unexpected errors use 500 without exposing internals.

Logout revokes the supplied refresh token; already issued access tokens remain valid until expiry.

## Images

Default: `Storage__Provider=Local`. PNG, JPEG, and WebP uploads up to 5 MB are checked by file signature, given generated filenames, stored under `01.API/uploads`, and served at `/uploads/{filename}`. URLs are relative to the API host. Signature checking does not decode/re-encode images.

Upload multipart fields: `file` and optional `primary=true`. With Cloudinary:

```powershell
$env:Storage__Provider = 'Cloudinary'
$env:Cloudinary__CloudName = 'YOUR_CLOUD'
$env:Cloudinary__ApiKey = 'YOUR_KEY'
$env:Cloudinary__ApiSecret = 'YOUR_SECRET'
```

Cloudinary integration compiles, but live cloud uploads were not exercised. Local image upload, serving, and deletion are covered by integration tests.

## Tests

The suite uses a real PostgreSQL server. It creates a uniquely named `clothesstore_tests_...` database, applies all migrations, runs HTTP requests against the application, and drops only that test database afterward. The configured role needs CREATEDB permission. It does not run tests against your application database.

```powershell
$env:CLOTHESSTORE_TEST_CONNECTION = 'Host=localhost;Username=postgres;Password=YOUR_PASSWORD;Database=postgres'
dotnet test ClothesStore.slnx
```

Alternatively, `.\scripts\test.ps1` discovers the .NET 10 SDK and uses `CLOTHESSTORE_TEST_CONNECTION`, then `ConnectionStrings__DefaultConnection`, then the existing appsettings connection. It never prints the connection string.

Coverage includes authorization, validation, refresh rotation/logout, order ownership, fulfillment transitions, atomic checkout rollback, concurrent checkout without overselling, cancellation/restocking, catalog archiving, category cycles, image lifecycle, OpenAPI, and health. Verification result: **10 passed, 0 failed**.

## Layout

- 01.API — controllers, authentication pipeline, exception handling, OpenAPI.
- 02. Application — request/response contracts and service/storage interfaces.
- 03. Infrastructure — EF/PostgreSQL persistence, Identity/JWT, store service, image providers, migrations.
- 04.Domain — entity models and order states.
- 05.Tests — HTTP integration tests backed by PostgreSQL.

Local setup completed: the application now targets ClothesStore on localhost:5432, and all three migrations have been applied. The pre-existing MyAppDb database was left unchanged.
