using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MarketPlaceApi.Models;

namespace MarketPlaceApi.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<RoastLevel> RoastLevel { get; set; }
        public DbSet<CoffeeProcess> CoffeeProcess { get; set; }
        public DbSet<CoffeeRegion> CoffeeRegion { get; set; }
        public DbSet<CoffeeProducer> CoffeeProducer { get; set; }
        public DbSet<CoffeeVarietal> CoffeeVarietal { get; set; }
        public DbSet<CoffeeAltitude> CoffeeAltitude { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Product>()
                .HasOne(p => p.Seller)
                .WithMany(u => u.Products)
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.Restrict); // avoid deleting products when user is deleted
        }
    }
}