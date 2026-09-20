using MarketPlaceApi.Controllers;
using MarketPlaceApi.Data;
using MarketPlaceApi.Models;
using MarketPlaceApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Npgsql;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Roaster's Market",
        Version = "1.0",
        Description = "The API for Roaster's Market"
    });
});

// Database Setup
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("DefaultConnection is empty. Check ConnectionStrings__DefaultConnection or appsettings.");
}

builder.Logging.AddSimpleConsole(options => options.SingleLine = true);
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "https://roastersmarket.vercel.app")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Identity Configuration
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("VerifiedSeller", policy =>
    {
        policy.RequireRole("Seller");
        policy.RequireClaim("is_verified", "true");
    });
});

// Controllers with JSON formatting
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Email Service (Local logging)
builder.Services.AddTransient<IEmailSender, ConsoleEmailSender>();

// Application Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICoffeeAttributeService, CoffeeAttributeService>();
builder.Services.AddScoped<IProductService, MarketPlaceApi.Services.ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IRoasterProfileService, RoasterProfileService>();
builder.Services.AddScoped<IStripeConnectService, StripeConnectService>();
builder.Services.AddScoped<MarketPlaceApi.Services.TokenService>();

// Cloudinary
builder.Services.Configure<CloudinaryOptions>(builder.Configuration.GetSection("Cloudinary"));
builder.Services.AddScoped<ICloudinarySigner, CloudinarySigner>();
builder.Services.AddScoped<IProductImagesService, ProductImagesService>();
builder.Services.AddScoped<ISellerImagesService, SellerImagesService>();

var app = builder.Build();

// Log database connection safely using the built app logger (fixes ASP0000)
try
{
    var parsed = new NpgsqlConnectionStringBuilder(connectionString);
    app.Logger.LogInformation("Using DB {Database} on {Host}:{Port} as {Username}.",
        parsed.Database, parsed.Host, parsed.Port, parsed.Username);
}
catch
{
    // Ignore parsing issues; let EF/Npgsql handle connectivity
}

// Database Migrations & Seeding (runs before receiving traffic)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { "Admin", "Seller", "Buyer" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));
    }

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    await MarketPlaceApi.Data.DataSeeder.SeedAsync(dbContext, userManager);

    var adminEmail = builder.Configuration["Admin:Email"] ?? "mostak1993@gmail.com";
    var adminPassword = builder.Configuration["Admin:Password"] ?? Environment.GetEnvironmentVariable("ADMIN_SEED_PASSWORD") ?? "MBdk6&N7Tl0P3n*Czi%=";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var newAdmin = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "Mostak",
            LastName = "Khan"
        };

        var result = await userManager.CreateAsync(newAdmin, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(newAdmin, "Admin");
        }
    }
}

// Middleware Pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "API is running!");
app.MapControllers();

app.Run();