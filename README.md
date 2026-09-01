# ☕ Roaster's Market API

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-10.0-512BD4?style=flat&logo=dotnet&logoColor=white)](https://learn.microsoft.com/en-us/aspnet/core/)
[![Entity Framework Core](https://img.shields.io/badge/EF_Core-10.0-512BD4?style=flat&logo=nuget&logoColor=white)](https://learn.microsoft.com/en-us/ef/core/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?style=flat&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Stripe](https://img.shields.io/badge/Stripe-Connect-635BFF?style=flat&logo=stripe&logoColor=white)](https://stripe.com/connect)
[![Cloudinary](https://img.shields.io/badge/Cloudinary-Media-3448C5?style=flat&logo=cloudinary&logoColor=white)](https://cloudinary.com/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=flat&logo=docker&logoColor=white)](https://www.docker.com/)

A modern, high-performance coffee marketplace e-commerce backend built with **ASP.NET Core (.NET 10)** and **PostgreSQL**. The platform empowers independent specialty coffee roasters to showcase their brand, manage fine-grained coffee listings with multi-variant packaging, accept customer payments via **Stripe Connect Express**, and fulfill orders, while offering coffee enthusiasts a tailored shopping experience with rich origin and roast metadata.

> **Live Web App:** [roastersmarket.vercel.app](https://roastersmarket.vercel.app/)  
> **Frontend Repository:** [marketplace-frontend](https://github.com/mossstak/marketplace-frontend) (Next.js & Tailwind CSS)  
> **Author:** [Mostak Khan](https://github.com/mossstak)

---

## 📑 Table of Contents

- [Key Features](#-key-features)
- [Architecture & Tech Stack](#-architecture--tech-stack)
- [Project Structure](#-project-structure)
- [Prerequisites](#-prerequisites)
- [Configuration & Environment Variables](#-configuration--environment-variables)
- [Installation & Getting Started](#-installation--getting-started)
  - [Local Development](#local-development)
  - [Docker & Docker Compose](#docker--docker-compose)
- [Database Migrations & Auto-Seeding](#-database-migrations--auto-seeding)
  - [Default Test Accounts](#default-test-accounts)
- [API Reference](#-api-reference)
  - [Authentication & Users (`/User`)](#authentication--users-user)
  - [Products & Catalog (`/Product`)](#products--catalog-product)
  - [Roaster Profiles (`/RoasterProfile`)](#roaster-profiles-roasterprofile)
  - [Image Upload & Management (`/seller/images`, `/products`)](#image-upload--management-sellerimages-products)
  - [Orders & Inventory (`/Order`)](#orders--inventory-order)
  - [Stripe Connect & Payments (`/api/StripeConnect`)](#stripe-connect--payments-apistripeconnect)
- [Running Tests](#-running-tests)
- [License](#-license)

---

## ✨ Key Features

- 🔐 **Authentication & Role-Based Access Control (RBAC)**
  - JWT Bearer authentication backed by **ASP.NET Core Identity**.
  - Three distinct user roles: **Admin**, **Seller** (Roaster), and **Buyer**.
  - Fine-grained permission policies (e.g., `VerifiedSeller`, owner-or-admin resource guards).

- ☕ **Specialty Coffee Domain Catalog**
  - Normalized, dynamic metadata attributes: **Roast Level**, **Processing Method**, **Origin Country**, **Region**, **Producer/Estate**, **Varietal**, and **Altitude (MASL)**.
  - Multi-variant packaging support (e.g., `250g`, `500g`, `1kg`) with independent stock counts and pricing (up to 6 variants per product).
  - Brewing method filters (**Beans**, **Espresso**, **Filter**, **French Press**) and product categories (**Coffee Beans**, **Grinders**, **Espresso Machines**, **Barista Tools**, **Misc**).

- 🏪 **Roaster Storefronts & Profiles**
  - Dedicated roaster profile pages featuring company bio, roasting philosophy, location, website, and social links (Instagram, TikTok, X, Facebook).
  - Admin-controlled **verification badge** system to establish seller credibility.

- 💳 **Stripe Connect Split Payments & Express Payouts**
  - Integrated **Stripe Connect Express** onboarding workflow allowing roasters to connect their payout bank accounts.
  - Express Dashboard single-sign-on login links for roasters to track their earnings.
  - Destination charge checkout with automated platform fee deduction (default **5% commission**).
  - Stripe webhook listener (`account.updated`) to synchronize payout enablement statuses in real time.

- 📸 **Cloudinary Media Pipeline**
  - Secure backend cryptographic signature generation (`/seller/images/sign`) enabling direct, signed client-to-Cloudinary uploads without exposing API secrets.
  - Multi-image association per product with customizable primary banner images.

- 📦 **Order Lifecycle & Real-Time Stock Management**
  - Atomic stock verification and deduction on checkout.
  - Complete order history for buyers and roasters.
  - Automated stock replenishment when orders are updated or deleted.
  - Status progression tracking: `Pending` ➔ `Paid` ➔ `Shipped` ➔ `Cancelled`.

- 🚀 **Zero-Config Database Seeding**
  - Automatic Entity Framework Core migrations applied on application boot.
  - Comprehensive seed data: Admin account, verified & unverified specialty roasters, buyer personas, coffee attribute lookup tables, and realistic specialty coffee offerings with variants.

---

## 🏗 Architecture & Tech Stack

| Layer | Technology | Description |
|---|---|---|
| **Runtime / SDK** | [.NET 10.0](https://dotnet.microsoft.com/) | Modern, high-performance C# runtime |
| **Web Framework** | [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/) | RESTful API controllers, dependency injection, and middleware |
| **Database** | [PostgreSQL 16](https://www.postgresql.org/) | Relational database engine |
| **ORM / Data Access** | [Entity Framework Core 10](https://learn.microsoft.com/en-us/ef/core/) / [Npgsql](https://www.npgsql.org/efcore/) | Code-first migrations, LINQ queries, and relational mapping |
| **Security & Auth** | ASP.NET Core Identity + JWT Bearer | Password hashing, claims-based authorization, and token validation |
| **Payments & Payouts** | [Stripe.net](https://github.com/stripe/stripe-dotnet) | Stripe Connect Express accounts, Destination Charges, and Webhooks |
| **Media Hosting** | [CloudinaryDotNet](https://cloudinary.com/documentation/dotnet_integration) | Cloud image storage and signed upload signatures |
| **API Documentation** | Swagger / OpenAPI ([Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)) | Interactive OpenAPI specification and Swagger UI |
| **Testing** | [xUnit](https://xunit.net/) + [Moq](https://github.com/devlooped/moq) | Unit testing and controller/service mocking |
| **Containerization** | Docker & Docker Compose | Containerized multi-service deployment |

---

## 📁 Project Structure

```
backend/
├── Controllers/                  # API endpoints and HTTP request routing
│   ├── OrderController.cs        # Order placement, status transitions, and tracking
│   ├── ProductController.cs      # Product catalog, search, and variant management
│   ├── ProductImagesController.cs# Product-to-image associations & primary image setup
│   ├── RoasterProfileController.cs# Roaster public storefronts & verification
│   ├── SellerImagesController.cs # Signed Cloudinary uploads and seller image catalog
│   ├── StripeConnectController.cs# Stripe Connect onboarding, payments, and webhooks
│   └── UserController.cs         # Registration, authentication, and user profile management
│
├── Services/                     # Core business logic and external integrations
│   ├── IUserService.cs / UserService.cs
│   ├── IProductService.cs / ProductService.cs
│   ├── IOrderService.cs / OrderService.cs
│   ├── IRoasterProfileService.cs / RoasterProfileService.cs
│   ├── IStripeConnectService.cs / StripeConnectService.cs
│   ├── IProductImagesService.cs / ProductImagesService.cs
│   ├── ISellerImagesService.cs / SellerImagesService.cs
│   ├── ICoffeeAttributeService.cs / CoffeeAttributeService.cs
│   ├── ICloudinarySigner.cs / CloudinarySigner.cs
│   └── TokenService.cs           # JWT creation and claim configuration
│
├── Models/                       # Entity Framework Core domain models
│   ├── User.cs                   # IdentityUser extension with profile & address
│   ├── Product.cs                # Product entity, coffee metadata entities, ProductVariant
│   ├── RoasterProfile.cs         # 1:1 Roaster storefront metadata & Stripe account info
│   ├── Order.cs / OrderItem.cs   # Order tracking, line items, and pricing
│   ├── ProductImage.cs           # Product image references
│   ├── SellerImage.cs            # Seller image library
│   ├── ProductCategory.cs        # Enum: CoffeeBeans, Grinder, EspressoMachine, etc.
│   ├── BrewingMethod.cs          # Enum: Beans, Espresso, Filter, FrenchPress
│   └── OrderStatus.cs            # Enum: Pending, Paid, Shipped, Cancelled
│
├── Dtos/                         # Strongly-typed Data Transfer Objects (DTOs)
│   ├── RegisterDto.cs / LoginDto.cs / EditUserDto.cs / UpdateUserDto.cs
│   ├── CreateProductDto.cs / EditProductDto.cs / UpdateProductDto.cs
│   ├── CreateOrderDto.cs / BuyVariantDto.cs / UpdateOrderStatusDto.cs
│   ├── RoasterProfileDto.cs / UpsertRoasterProfileDto.cs / VerifyRoasterProfileDto.cs
│   └── StripeConnectDtos.cs     # Onboarding, payout status, and destination payment DTOs
│
├── Data/                         # Database persistence and seeding
│   ├── ApplicationDbContext.cs   # EF Core DbContext with relationship definitions
│   └── DataSeeder.cs             # Automated seeder for roles, admin, roasters, and products
│
├── Migrations/                   # EF Core database migration history
├── MarketPlaceApi.Tests/         # xUnit unit test project with Moq fixtures
│   ├── UserControllerTests.cs
│   ├── UserServiceTests.cs
│   └── MarketPlaceApi.Tests.csproj
│
├── Dockerfile                    # Multi-stage Docker build file
├── docker-compose.yml            # Local orchestration for PostgreSQL + API
├── MarketPlaceApi.csproj         # Project configuration and NuGet dependencies
├── MarketPlaceApi.http           # Visual Studio / VS Code REST Client test requests
├── Program.cs                    # Application bootstrapping, DI container, and middleware
├── appsettings.json              # Base configuration template
└── appsettings.Development.json  # Local development overrides
```

---

## ⚙️ Prerequisites

- **[.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)** or higher
- **[PostgreSQL 16](https://www.postgresql.org/download/)** (or Docker)
- **[Cloudinary Account](https://cloudinary.com/)** (for image upload signing)
- **[Stripe Account](https://stripe.com/)** (with Connect enabled for platform payouts)
- **[Docker Desktop](https://www.docker.com/products/docker-desktop/)** *(optional, for containerized run)*

---

## 🔧 Configuration & Environment Variables

Configure application settings in `appsettings.Development.json`, `.NET User Secrets`, or environment variables.

### Configuration Template (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=MarketPlaceApi;Username=postgres;Password=YourSecurePassword"
  },
  "Jwt": {
    "Key": "YOUR_STRONG_SECRET_KEY_MIN_32_CHARACTERS_LONG",
    "Issuer": "MarketPlaceApi",
    "Audience": "MarketPlaceApiUsers",
    "DurationInMinutes": 60
  },
  "Stripe": {
    "SecretKey": "sk_test_YOUR_STRIPE_SECRET_KEY",
    "PublishableKey": "pk_test_YOUR_STRIPE_PUBLISHABLE_KEY",
    "WebhookSecret": "whsec_YOUR_STRIPE_WEBHOOK_SECRET"
  },
  "Cloudinary": {
    "CloudName": "YOUR_CLOUDINARY_CLOUD_NAME",
    "ApiKey": "YOUR_CLOUDINARY_API_KEY",
    "ApiSecret": "YOUR_CLOUDINARY_API_SECRET"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Environment Variable Mapping

| Environment Variable | Description | Default / Example |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | `Host=localhost;Port=5432;Database=MarketPlaceApi;Username=postgres;Password=...` |
| `Jwt__Key` | Secret key for signing JWT tokens (min 32 chars) | `your_secret_key_min_32_chars` |
| `Jwt__Issuer` | JWT token issuer | `MarketPlaceApi` |
| `Jwt__Audience` | JWT token audience | `MarketPlaceApiUsers` |
| `Jwt__DurationInMinutes` | Token expiration lifetime in minutes | `60` |
| `Stripe__SecretKey` | Stripe Secret API Key | `sk_test_...` |
| `Stripe__PublishableKey` | Stripe Publishable API Key | `pk_test_...` |
| `Stripe__WebhookSecret` | Stripe Webhook signing secret | `whsec_...` |
| `Cloudinary__CloudName` | Cloudinary Cloud name identifier | `your_cloud_name` |
| `Cloudinary__ApiKey` | Cloudinary API Key | `your_api_key` |
| `Cloudinary__ApiSecret` | Cloudinary API Secret | `your_api_secret` |
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Development` or `Production` |

> [!TIP]
> In local development, you can safely store secrets using the .NET Secret Manager:
> ```bash
> dotnet user-secrets set "Stripe:SecretKey" "sk_test_..."
> dotnet user-secrets set "Cloudinary:ApiSecret" "..."
> ```

---

## 🚀 Installation & Getting Started

### Local Development

#### 1. Clone the repository
```bash
git clone https://github.com/mossstak/marketplace-api.git
cd marketplace-api
```

#### 2. Restore NuGet dependencies
```bash
dotnet restore
```

#### 3. Setup PostgreSQL
Ensure PostgreSQL is running locally on port `5432` with a database named `MarketPlaceApi` (or match the credentials in your `appsettings.Development.json`).

#### 4. Apply Database Migrations
Run the EF Core migration command to initialize the database schema:
```bash
dotnet ef database update
```

#### 5. Run the Application
```bash
dotnet run
```
The API server will start. By default, it listens at:
- **API Base:** `http://localhost:5000` (or `http://localhost:5131`)
- **Swagger UI:** `http://localhost:5000/swagger`

---

### Docker & Docker Compose

Run the API and PostgreSQL together in isolated containers with health checks:

```bash
docker-compose up --build
```

- **API:** `http://localhost:5000`
- **PostgreSQL:** `localhost:5433` (mapped from container port 5432)
- **Data Persistence:** Stored in the `marketplace_pgdata` Docker volume

To stop the containers:
```bash
docker-compose down
```

---

## 🗄 Database Migrations & Auto-Seeding

When the application boots up, `Program.cs` automatically executes pending EF Core migrations and triggers `DataSeeder.SeedAsync()` to populate lookup tables and realistic demonstration accounts.

### Default Test Accounts

| Role | Email | Password | Details |
|---|---|---|---|
| **Admin** | `mostak1993@gmail.com` | `MBdk6&N7Tl0P3n*Czi%=` | Full platform administration |
| **Seller** | `sarah@bloomroasters.com` | `Bloom@1234` | Bloom Roasters (London) — Verified |
| **Seller** | `marcus@pacificcrestcoffee.com` | `Pacific@1234` | Pacific Crest Coffee (Seattle) — Verified |
| **Seller** | `amara@ubunturoasters.com` | `Ubuntu@1234` | Ubuntu Roasters (Cape Town) — Unverified |
| **Buyer** | `james.williams@email.com` | `Buyer@1234` | Manchester, UK |
| **Buyer** | `priya.patel@email.com` | `Buyer@1234` | Birmingham, UK |
| **Buyer** | `lars.andersen@email.com` | `Buyer@1234` | Copenhagen, Denmark |
| **Buyer** | `sofia.moreno@email.com` | `Buyer@1234` | Barcelona, Spain |
| **Buyer** | `daniel.kim@email.com` | `Buyer@1234` | Seoul, South Korea |

---

## 📡 API Reference

### Authentication & Users (`/User`)

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/User/register` | Public | Register a new user (`Buyer` or `Seller` role) with full address details |
| `POST` | `/User/login` | Public | Authenticate user and receive JWT Bearer token and assigned roles |
| `GET` | `/User/me` | Logged In | Retrieve the authenticated user's profile and roles |
| `GET` | `/User/all` | Admin | Retrieve a list of all registered platform users |
| `PATCH` | `/User/edituser/{id}` | Self / Admin | Partially update user details (name, email, address) |
| `PUT` | `/User/updateuser/{id}` | Self / Admin | Full update of user profile details |
| `POST` | `/User/change-password` | Logged In | Change password with current password verification |
| `POST` | `/User/reset-password/{id}` | Admin | Administrative password reset |
| `DELETE` | `/User/delete/{id}` | Admin | Delete a user account |

---

### Products & Catalog (`/Product`)

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/Product/all` | Public | Fetch all available products with variants and coffee metadata |
| `GET` | `/Product/{id}` | Public | Fetch single product details by ID |
| `GET` | `/Product/me` | Seller | Retrieve all products listed by the currently authenticated seller |
| `POST` | `/Product/addproduct` | Seller, Admin | Create a new coffee product listing with variants and attributes |
| `PATCH` | `/Product/editproduct/{id}`| Seller, Admin | Partially update product metadata |
| `PUT` | `/Product/updateproduct/{id}`| Seller, Admin | Full update of product details and variants |
| `PATCH` | `/Product/variant/{variantId}`| Seller, Admin | Update individual variant pricing, size, or stock quantity |
| `DELETE` | `/Product/delete/{id}` | Seller, Admin | Delete a product listing |

---

### Roaster Profiles (`/RoasterProfile`)

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `GET` | `/RoasterProfile/all` | Public | List all roaster storefront profiles |
| `GET` | `/RoasterProfile/{userId}` | Public | Get public storefront profile for a specific roaster |
| `GET` | `/RoasterProfile/me` | Logged In | Get currently authenticated seller's roaster profile |
| `PUT` | `/RoasterProfile/me` | Logged In | Upsert roaster profile details (bio, city, country, socials) |
| `POST` | `/RoasterProfile/verify/{userId}` | Admin | Toggle seller verification badge (`isVerified: true/false`) |

---

### Image Upload & Management (`/seller/images`, `/products`)

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/seller/images/sign` | Seller, Admin | Generate cryptographic signature parameters for Cloudinary direct upload |
| `POST` | `/seller/images` | Seller, Admin | Save image metadata to seller's asset library after upload |
| `GET` | `/seller/images` | Seller, Admin | List all uploaded images belonging to the current seller |
| `DELETE` | `/seller/images/{imageId}` | Seller, Admin | Delete an image from the seller's asset library |
| `POST` | `/products/{productId}/images/attach` | Seller, Admin | Attach a seller library image to a specific product |
| `PATCH` | `/products/{productId}/images/{imageId}/primary` | Seller, Admin | Set a specific product image as the primary cover image |
| `DELETE` | `/products/{productId}/images/{imageId}` | Seller, Admin | Remove an image attachment from a product |

---

### Orders & Inventory (`/Order`)

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/Order/place` | Buyer | Place an order; validates stock and deducts variant inventory |
| `GET` | `/Order/mine` | Logged In | Retrieve purchase history for the logged-in buyer |
| `GET` | `/Order/seller` | Seller | Retrieve incoming orders containing items sold by the seller |
| `PUT` | `/Order/update/{id}` | Buyer | Modify items on an existing order with stock readjustment |
| `PATCH` | `/Order/{id}/status` | Seller, Admin | Update order status (`Pending`, `Paid`, `Shipped`, `Cancelled`) |
| `DELETE` | `/Order/delete/{id}` | Buyer, Admin | Delete/cancel order and restore item inventory |

---

### Stripe Connect & Payments (`/api/StripeConnect`)

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| `POST` | `/api/StripeConnect/onboarding-link` | Seller, Admin | Create or retrieve Stripe Express onboarding URL for seller payout setup |
| `GET` | `/api/StripeConnect/account-status` | Seller, Admin | Check charges and payouts enablement status on connected Stripe account |
| `POST` | `/api/StripeConnect/login-link` | Seller, Admin | Generate single-sign-on login URL to the Stripe Express dashboard |
| `POST` | `/api/StripeConnect/create-payment-intent` | Public | Create Destination PaymentIntent with automatic platform commission |
| `POST` | `/api/StripeConnect/webhook` | Public | Stripe webhook endpoint handling asynchronous events (`account.updated`) |

---

## 🧪 Running Tests

The test suite is built using **xUnit** and **Moq** to test business logic and controller endpoints.

To run all automated tests:

```bash
dotnet test
```

For verbose output with test names:
```bash
dotnet test --logger "console;verbosity=detailed"
```

---

## 📄 License

This project is licensed under the [MIT License](LICENSE).

---

<div align="center">
  <sub>Built with ❤️ by <a href="https://github.com/mossstak">Mostak Khan</a></sub>
</div>