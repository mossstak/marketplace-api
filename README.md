# Marketplace API

A coffee marketplace e-commerce backend built with ASP.NET Core (.NET 10). Sellers (roasters) can list and manage their coffee products, buyers can browse and purchase, and admins oversee the platform.

> Created by [Mostak Khan](https://github.com/mossstak) — built to combine a passion for coffee with backend development. The frontend (Next.js) lives at [marketplace-frontend](https://github.com/mossstak/marketplace-frontend).

## Features

- **User management** — register/login with JWT authentication, role-based access (Admin, Seller, Buyer)
- **Products** — create and manage coffee listings with detailed attributes (roast level, origin, process, varietal, etc.) and up to 6 size/price variants
- **Orders** — place and track orders with stock validation
- **Payments** — Stripe payment intents and webhook handling
- **Image uploads** — Cloudinary integration for product and seller images
- **Roaster profiles** — seller storefronts with bio, location, and social links; admin-controlled verification badge

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core (.NET 10) |
| Database | PostgreSQL |
| ORM | Entity Framework Core 10 |
| Auth | ASP.NET Identity + JWT Bearer |
| Payments | Stripe |
| Image Hosting | Cloudinary |
| API Docs | Swagger / OpenAPI |
| Testing | xUnit + Moq |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL (or Docker)
- Stripe account (test keys are fine for local dev)
- Cloudinary account

## Getting Started

### 1. Clone and restore

```bash
git clone <repo-url>
cd backend
dotnet restore
```

### 2. Configure environment

Fill in your credentials in `appsettings.Development.json` (or use [.NET user secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=MarketPlaceApi;Username=postgres;Password=yourpassword"
  },
  "Jwt": {
    "Key": "your_secret_key_min_32_chars",
    "Issuer": "MarketPlaceApi",
    "Audience": "MarketPlaceApiUsers",
    "DurationInMinutes": 60
  },
  "Stripe": {
    "SecretKey": "sk_test_...",
    "PublishableKey": "pk_test_..."
  },
  "Cloudinary": {
    "CloudName": "your_cloud_name",
    "ApiKey": "your_api_key",
    "ApiSecret": "your_api_secret"
  }
}
```

### 3. Run migrations and start

```bash
dotnet ef database update
dotnet run
```

The API will be available at `http://localhost:5000`. Swagger UI is at `http://localhost:5000/swagger`.

### Docker (alternative)

Runs both PostgreSQL and the API together:

```bash
docker-compose up
```

- API: `http://localhost:5000`
- PostgreSQL: `localhost:5433`

## Project Structure

```
backend/
├── Controllers/          # API endpoints
├── Services/             # Business logic (with interfaces)
├── Models/               # EF Core entity models
├── Dtos/                 # Request/response data shapes
├── Data/                 # ApplicationDbContext
├── Migrations/           # EF Core migration history
├── MarketPlaceApi.Tests/ # xUnit test project
├── Program.cs            # App startup and DI configuration
├── Dockerfile
└── docker-compose.yml
```

## API Overview

| Area | Base Route | Auth |
|---|---|---|
| Users | `/api/user` | Mixed |
| Products | `/api/product` | Mixed |
| Orders | `/api/order` | Required |
| Payments | `/api/payment` | Required |
| Roaster Profiles | `/api/roasterprofile` | Mixed |
| Product Images | `/api/productimages` | Seller |
| Seller Images | `/api/sellerimages` | Seller |

### Roles

- **Admin** — verify sellers, manage users, update order status
- **Seller** — create/edit products, upload images, manage own orders
- **Buyer** — browse products, place and track orders

Sellers must have a complete roaster profile and be verified by an Admin before they can list products.

## Running Tests

```bash
dotnet test
```

Tests use xUnit with Moq for mocking and an in-memory database for integration-style tests.

## Environment Variables Reference

| Variable | Description |
|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `Jwt__Key` | JWT signing secret (min 32 chars) |
| `Jwt__Issuer` | Token issuer |
| `Jwt__Audience` | Token audience |
| `Jwt__DurationInMinutes` | Token lifetime (default: 60) |
| `Stripe__SecretKey` | Stripe secret key |
| `Stripe__PublishableKey` | Stripe publishable key |
| `Cloudinary__CloudName` | Cloudinary cloud name |
| `Cloudinary__ApiKey` | Cloudinary API key |
| `Cloudinary__ApiSecret` | Cloudinary API secret |
| `ASPNETCORE_ENVIRONMENT` | `Development` or `Production` |

## Notes

- Prices are in minor units (e.g. `1299` = £12.99 GBP)
- Each product supports up to 6 size variants
- CORS is configured for `localhost:5173` and `localhost:3000`
- Swagger UI is only enabled in the Development environment