using MarketPlaceApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarketPlaceApi.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db, UserManager<User> userManager)
        {
            await SeedAdminAsync(userManager);
            await SeedCoffeeAttributesAsync(db);
            var sellerIds = await SeedSellersAsync(userManager, db);
            await SeedBuyersAsync(userManager);
            await SeedProductsAsync(db, sellerIds);
        }

        // ── Coffee lookup tables ──────────────────────────────────────────────

        private static async Task SeedCoffeeAttributesAsync(ApplicationDbContext db)
        {
            if (!await db.RoastLevel.AnyAsync())
            {
                db.RoastLevel.AddRange(
                    new RoastLevel { Name = "Light" },
                    new RoastLevel { Name = "Medium" },
                    new RoastLevel { Name = "Medium-Dark" },
                    new RoastLevel { Name = "Dark" }
                );
            }

            if (!await db.CoffeeProcess.AnyAsync())
            {
                db.CoffeeProcess.AddRange(
                    new CoffeeProcess { Name = "Washed" },
                    new CoffeeProcess { Name = "Natural" },
                    new CoffeeProcess { Name = "Honey" },
                    new CoffeeProcess { Name = "Anaerobic" }
                );
            }

            if (!await db.CoffeeOrigin.AnyAsync())
            {
                db.CoffeeOrigin.AddRange(
                    new CoffeeOrigin { Name = "Ethiopia" },
                    new CoffeeOrigin { Name = "Colombia" },
                    new CoffeeOrigin { Name = "Brazil" },
                    new CoffeeOrigin { Name = "Guatemala" },
                    new CoffeeOrigin { Name = "Kenya" }
                );
            }

            if (!await db.CoffeeRegion.AnyAsync())
            {
                db.CoffeeRegion.AddRange(
                    new CoffeeRegion { Name = "Yirgacheffe" },
                    new CoffeeRegion { Name = "Huila" },
                    new CoffeeRegion { Name = "Cerrado" },
                    new CoffeeRegion { Name = "Antigua" },
                    new CoffeeRegion { Name = "Nyeri" }
                );
            }

            if (!await db.CoffeeProducer.AnyAsync())
            {
                db.CoffeeProducer.AddRange(
                    new CoffeeProducer { Name = "Jabez Tadesse" },
                    new CoffeeProducer { Name = "Luis Anibal Calderon" },
                    new CoffeeProducer { Name = "Daterra Farm" },
                    new CoffeeProducer { Name = "El Injerto" },
                    new CoffeeProducer { Name = "Gakuyuini Estate" }
                );
            }

            if (!await db.CoffeeVarietal.AnyAsync())
            {
                db.CoffeeVarietal.AddRange(
                    new CoffeeVarietal { Name = "Heirloom" },
                    new CoffeeVarietal { Name = "Caturra" },
                    new CoffeeVarietal { Name = "Bourbon" },
                    new CoffeeVarietal { Name = "Typica" },
                    new CoffeeVarietal { Name = "SL28" }
                );
            }

            if (!await db.CoffeeAltitude.AnyAsync())
            {
                db.CoffeeAltitude.AddRange(
                    new CoffeeAltitude { ValueInMasl = 1200 },
                    new CoffeeAltitude { ValueInMasl = 1500 },
                    new CoffeeAltitude { ValueInMasl = 1800 },
                    new CoffeeAltitude { ValueInMasl = 2100 },
                    new CoffeeAltitude { ValueInMasl = 2400 }
                );
            }

            await db.SaveChangesAsync();
        }

        private static async Task SeedAdminAsync(UserManager<User> userManager)
        {
            const string email = "mostak1993@gmail.com";
            const string password = "MBdk6&N7Tl0P3n*Czi%=";

            if (await userManager.FindByEmailAsync(email) != null) return;

            var user = new User
            {
                UserName = email,
                Email = email,
                FirstName = "Mostak",
                LastName = "Khan",
                EmailConfirmed = true,
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new Exception($"Failed to create admin {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            await userManager.AddToRoleAsync(user, "Admin");
        }

        // ── Seller accounts + roaster profiles ───────────────────────────────

        private static async Task<List<string>> SeedSellersAsync(UserManager<User> userManager, ApplicationDbContext db)
        {
            var sellers = new[]
            {
                new
                {
                    Email       = "sarah@bloomroasters.com",
                    Password    = "Bloom@1234",
                    FirstName   = "Sarah",
                    LastName    = "Johnson",
                    AddressOne  = "12 Roast Lane",
                    City        = "London",
                    Country     = "United Kingdom",
                    PostalCode  = "EC1A 1BB",
                    CompanyName = "Bloom Roasters",
                    Bio         = "Specialty micro-roastery in the heart of London, sourcing single-origin lots from Ethiopia and Colombia.",
                    WebsiteUrl  = "https://bloomroasters.co.uk",
                    Instagram   = "https://instagram.com/bloomroasters",
                    IsVerified  = true,
                },
                new
                {
                    Email       = "marcus@pacificcrestcoffee.com",
                    Password    = "Pacific@1234",
                    FirstName   = "Marcus",
                    LastName    = "Chen",
                    AddressOne  = "88 Pike Street",
                    City        = "Seattle",
                    Country     = "United States",
                    PostalCode  = "98101",
                    CompanyName = "Pacific Crest Coffee",
                    Bio         = "Pacific Northwest roaster obsessed with bright, fruit-forward Kenyan and Ethiopian coffees.",
                    WebsiteUrl  = "https://pacificcrestcoffee.com",
                    Instagram   = "https://instagram.com/pacificcrestcoffee",
                    IsVerified  = true,
                },
                new
                {
                    Email       = "amara@ubunturoasters.com",
                    Password    = "Ubuntu@1234",
                    FirstName   = "Amara",
                    LastName    = "Osei",
                    AddressOne  = "5 Buitenkant Street",
                    City        = "Cape Town",
                    Country     = "South Africa",
                    PostalCode  = "8001",
                    CompanyName = "Ubuntu Roasters",
                    Bio         = "Celebrating African coffee heritage one cup at a time. Direct-trade with smallholder farmers across the continent.",
                    WebsiteUrl  = "https://ubunturoasters.co.za",
                    Instagram   = "https://instagram.com/ubunturoasters",
                    IsVerified  = false,
                },
            };

            var ids = new List<string>();

            foreach (var s in sellers)
            {
                var existing = await userManager.FindByEmailAsync(s.Email);
                if (existing != null)
                {
                    ids.Add(existing.Id);
                    continue;
                }

                var user = new User
                {
                    UserName = s.Email,
                    Email = s.Email,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    AddressOne = s.AddressOne,
                    City = s.City,
                    Country = s.Country,
                    PostalCode = s.PostalCode,
                    EmailConfirmed = true,
                };

                var result = await userManager.CreateAsync(user, s.Password);
                if (!result.Succeeded)
                    throw new Exception($"Failed to create seller {s.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");

                await userManager.AddToRoleAsync(user, "Seller");
                ids.Add(user.Id);

                db.RoasterProfiles.Add(new RoasterProfile
                {
                    UserId = user.Id,
                    CompanyName = s.CompanyName,
                    Bio = s.Bio,
                    City = s.City,
                    Country = s.Country,
                    IsVerified = s.IsVerified,
                    VerifiedAtUtc = s.IsVerified ? DateTime.UtcNow.AddMonths(-3) : null,
                    WebsiteUrl = s.WebsiteUrl,
                    InstagramUrl = s.Instagram,
                });
            }

            await db.SaveChangesAsync();
            return ids;
        }

        // ── Buyer accounts ────────────────────────────────────────────────────

        private static async Task SeedBuyersAsync(UserManager<User> userManager)
        {
            var buyers = new[]
            {
                new
                {
                    Email = "james.williams@email.com",
                    Password = "Buyer@1234",
                    FirstName = "James",
                    LastName = "Williams",
                    City = "Manchester",
                    Country = "United Kingdom",
                    PostalCode = "M1 1AE"
                },
                new 
                { 
                    Email = "priya.patel@email.com",     
                    Password = "Buyer@1234", 
                    FirstName = "Priya",  
                    LastName = "Patel",    
                    City = "Birmingham",  
                    Country = "United Kingdom", 
                    PostalCode = "B1 1BB" 
                },
                new 
                { 
                    Email = "lars.andersen@email.com",   
                    Password = "Buyer@1234", 
                    FirstName = "Lars",   
                    LastName = "Andersen", 
                    City = "Copenhagen",  
                    Country = "Denmark",         
                    PostalCode = "1050"   
                },
                new 
                { 
                    Email = "sofia.moreno@email.com",    
                    Password = "Buyer@1234", 
                    FirstName = "Sofia",  
                    LastName = "Moreno",   
                    City = "Barcelona",   
                    Country = "Spain",           
                    PostalCode = "08001"  
                },
                new 
                { 
                    Email = "daniel.kim@email.com",      
                    Password = "Buyer@1234", 
                    FirstName = "Daniel", 
                    LastName = "Kim",      
                    City = "Seoul",       
                    Country = "South Korea",     
                    PostalCode = "03187"  
                },
            };

            foreach (var b in buyers)
            {
                if (await userManager.FindByEmailAsync(b.Email) != null) continue;

                var user = new User
                {
                    UserName = b.Email,
                    Email = b.Email,
                    FirstName = b.FirstName,
                    LastName = b.LastName,
                    City = b.City,
                    Country = b.Country,
                    PostalCode = b.PostalCode,
                    EmailConfirmed = true,
                };

                var result = await userManager.CreateAsync(user, b.Password);
                if (!result.Succeeded)
                    throw new Exception($"Failed to create buyer {b.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");

                await userManager.AddToRoleAsync(user, "Buyer");
            }
        }

        // ── Products ──────────────────────────────────────────────────────────

        private static async Task SeedProductsAsync(ApplicationDbContext db, List<string> sellerIds)
        {
            if (await db.Products.AnyAsync()) return;

            // Load attribute IDs that were just seeded
            var roastLight = (await db.RoastLevel.FirstAsync(r => r.Name == "Light")).Id;
            var roastMedium = (await db.RoastLevel.FirstAsync(r => r.Name == "Medium")).Id;
            var roastMediumDark = (await db.RoastLevel.FirstAsync(r => r.Name == "Medium-Dark")).Id;
            var roastDark = (await db.RoastLevel.FirstAsync(r => r.Name == "Dark")).Id;

            var procWashed = (await db.CoffeeProcess.FirstAsync(p => p.Name == "Washed")).Id;
            var procNatural = (await db.CoffeeProcess.FirstAsync(p => p.Name == "Natural")).Id;
            var procHoney = (await db.CoffeeProcess.FirstAsync(p => p.Name == "Honey")).Id;
            var procAnaerobic = (await db.CoffeeProcess.FirstAsync(p => p.Name == "Anaerobic")).Id;

            var origEthiopia = (await db.CoffeeOrigin.FirstAsync(o => o.Name == "Ethiopia")).Id;
            var origColombia = (await db.CoffeeOrigin.FirstAsync(o => o.Name == "Colombia")).Id;
            var origBrazil = (await db.CoffeeOrigin.FirstAsync(o => o.Name == "Brazil")).Id;
            var origGuatemala = (await db.CoffeeOrigin.FirstAsync(o => o.Name == "Guatemala")).Id;
            var origKenya = (await db.CoffeeOrigin.FirstAsync(o => o.Name == "Kenya")).Id;

            var regYirgacheffe = (await db.CoffeeRegion.FirstAsync(r => r.Name == "Yirgacheffe")).Id;
            var regHuila = (await db.CoffeeRegion.FirstAsync(r => r.Name == "Huila")).Id;
            var regCerrado = (await db.CoffeeRegion.FirstAsync(r => r.Name == "Cerrado")).Id;
            var regAntigua = (await db.CoffeeRegion.FirstAsync(r => r.Name == "Antigua")).Id;
            var regNyeri = (await db.CoffeeRegion.FirstAsync(r => r.Name == "Nyeri")).Id;

            var prodJabez = (await db.CoffeeProducer.FirstAsync(p => p.Name == "Jabez Tadesse")).Id;
            var prodLuis = (await db.CoffeeProducer.FirstAsync(p => p.Name == "Luis Anibal Calderon")).Id;
            var prodDaterra = (await db.CoffeeProducer.FirstAsync(p => p.Name == "Daterra Farm")).Id;
            var prodElInjerto = (await db.CoffeeProducer.FirstAsync(p => p.Name == "El Injerto")).Id;
            var prodGakuyuini = (await db.CoffeeProducer.FirstAsync(p => p.Name == "Gakuyuini Estate")).Id;

            var varHeirloom = (await db.CoffeeVarietal.FirstAsync(v => v.Name == "Heirloom")).Id;
            var varCaturra = (await db.CoffeeVarietal.FirstAsync(v => v.Name == "Caturra")).Id;
            var varBourbon = (await db.CoffeeVarietal.FirstAsync(v => v.Name == "Bourbon")).Id;
            var varTypica = (await db.CoffeeVarietal.FirstAsync(v => v.Name == "Typica")).Id;
            var varSL28 = (await db.CoffeeVarietal.FirstAsync(v => v.Name == "SL28")).Id;

            var alt1200 = (await db.CoffeeAltitude.FirstAsync(a => a.ValueInMasl == 1200)).Id;
            var alt1500 = (await db.CoffeeAltitude.FirstAsync(a => a.ValueInMasl == 1500)).Id;
            var alt1800 = (await db.CoffeeAltitude.FirstAsync(a => a.ValueInMasl == 1800)).Id;
            var alt2100 = (await db.CoffeeAltitude.FirstAsync(a => a.ValueInMasl == 2100)).Id;
            var alt2400 = (await db.CoffeeAltitude.FirstAsync(a => a.ValueInMasl == 2400)).Id;

            // seller 0 = Sarah (Bloom Roasters)
            // seller 1 = Marcus (Pacific Crest)
            // seller 2 = Amara (Ubuntu)
            string sarahId = sellerIds[0];
            string marcusId = sellerIds[1];
            string amaraId = sellerIds[2];

            var products = new List<Product>
            {
                // ── Sarah – Bloom Roasters ────────────────────────────────────
                new Product
                {
                    ProductName        = "Ethiopian Yirgacheffe Natural",
                    ProductDescription = "A vibrant natural-processed gem from the Yirgacheffe highlands. Bursting with blueberry jam, dark chocolate, and a hint of jasmine florals.",
                    Category           = ProductCategory.CoffeeBeans,
                    Grind              = BrewingMethod.Beans,
                    RoastLevelId       = roastLight,
                    CoffeeProcessId    = procNatural,
                    OriginId           = origEthiopia,
                    RegionId           = regYirgacheffe,
                    ProducerId         = prodJabez,
                    VarietalId         = varHeirloom,
                    AltitudeId         = alt2100,
                    TastingNotes       = "Blueberry, Dark Chocolate, Jasmine",
                    RoastDate          = DateTime.UtcNow.AddDays(-7),
                    SellerId           = sarahId,
                    Variants           = Variants(9.50m, 17.00m, 30.00m),
                },
                new Product
                {
                    ProductName        = "Colombian Honey Process",
                    ProductDescription = "Sourced from Luis Anibal Calderon's farm in Huila, this honey-processed Caturra delivers a rich caramel sweetness balanced by crisp red-apple acidity.",
                    Category           = ProductCategory.CoffeeBeans,
                    Grind              = BrewingMethod.Filter,
                    RoastLevelId       = roastMedium,
                    CoffeeProcessId    = procHoney,
                    OriginId           = origColombia,
                    RegionId           = regHuila,
                    ProducerId         = prodLuis,
                    VarietalId         = varCaturra,
                    AltitudeId         = alt1800,
                    TastingNotes       = "Caramel, Red Apple, Brown Sugar",
                    RoastDate          = DateTime.UtcNow.AddDays(-5),
                    SellerId           = sarahId,
                    Variants           = Variants(8.50m, 15.50m, 27.00m),
                },
                new Product
                {
                    ProductName        = "Brazilian Cerrado Espresso Blend",
                    ProductDescription = "Full-bodied and low-acid. Daterra's Bourbon-heavy lots bring notes of milk chocolate, toasted nuts, and a lingering molasses finish — perfect as a shot or in milk.",
                    Category           = ProductCategory.CoffeeBeans,
                    Grind              = BrewingMethod.Espresso,
                    RoastLevelId       = roastDark,
                    CoffeeProcessId    = procNatural,
                    OriginId           = origBrazil,
                    RegionId           = regCerrado,
                    ProducerId         = prodDaterra,
                    VarietalId         = varBourbon,
                    AltitudeId         = alt1200,
                    TastingNotes       = "Milk Chocolate, Toasted Almond, Molasses",
                    RoastDate          = DateTime.UtcNow.AddDays(-3),
                    SellerId           = sarahId,
                    Variants           = Variants(7.50m, 13.50m, 24.00m),
                },

                // ── Marcus – Pacific Crest Coffee ─────────────────────────────
                new Product
                {
                    ProductName        = "Kenyan Nyeri AA Washed",
                    ProductDescription = "Classic Kenyan brightness from the Gakuyuini Estate. Expect explosive blackcurrant, grapefruit zest, and a long wine-like finish on a silky body.",
                    Category           = ProductCategory.CoffeeBeans,
                    Grind              = BrewingMethod.Beans,
                    RoastLevelId       = roastLight,
                    CoffeeProcessId    = procWashed,
                    OriginId           = origKenya,
                    RegionId           = regNyeri,
                    ProducerId         = prodGakuyuini,
                    VarietalId         = varSL28,
                    AltitudeId         = alt1800,
                    TastingNotes       = "Blackcurrant, Grapefruit, Red Wine",
                    RoastDate          = DateTime.UtcNow.AddDays(-10),
                    SellerId           = marcusId,
                    Variants           = Variants(11.00m, 20.00m, 36.00m),
                },
                new Product
                {
                    ProductName        = "Guatemala Antigua Washed",
                    ProductDescription = "Grown at high altitude on volcanic soils, El Injerto's Typica brings a gentle sweetness of milk chocolate and dried fruit with a clean, crisp finish.",
                    Category           = ProductCategory.CoffeeBeans,
                    Grind              = BrewingMethod.FrenchPress,
                    RoastLevelId       = roastMedium,
                    CoffeeProcessId    = procWashed,
                    OriginId           = origGuatemala,
                    RegionId           = regAntigua,
                    ProducerId         = prodElInjerto,
                    VarietalId         = varTypica,
                    AltitudeId         = alt1500,
                    TastingNotes       = "Milk Chocolate, Dried Fig, Hazelnut",
                    RoastDate          = DateTime.UtcNow.AddDays(-8),
                    SellerId           = marcusId,
                    Variants           = Variants(8.00m, 14.50m, 26.00m),
                },

                // ── Amara – Ubuntu Roasters ───────────────────────────────────
                new Product
                {
                    ProductName        = "Ethiopian Anaerobic Sidama",
                    ProductDescription = "An experimental anaerobic lot pushing the boundaries of Ethiopian coffee. Think tropical punch, strawberry candy, and a velvety mouthfeel that lingers long after the cup.",
                    Category           = ProductCategory.CoffeeBeans,
                    Grind              = BrewingMethod.Beans,
                    RoastLevelId       = roastMediumDark,
                    CoffeeProcessId    = procAnaerobic,
                    OriginId           = origEthiopia,
                    RegionId           = regYirgacheffe,
                    ProducerId         = prodJabez,
                    VarietalId         = varHeirloom,
                    AltitudeId         = alt2400,
                    TastingNotes       = "Strawberry, Tropical Fruit, Fermented Plum",
                    RoastDate          = DateTime.UtcNow.AddDays(-4),
                    SellerId           = amaraId,
                    Variants           = Variants(12.00m, 22.00m, 40.00m),
                },
                new Product
                {
                    ProductName        = "Colombian Huila Washed Caturra",
                    ProductDescription = "A textbook Colombian from the Huila region. Bright and clean with a sweet citrus backbone, honey sweetness, and a light, tea-like body — great for filter brewing.",
                    Category           = ProductCategory.CoffeeBeans,
                    Grind              = BrewingMethod.Filter,
                    RoastLevelId       = roastMedium,
                    CoffeeProcessId    = procWashed,
                    OriginId           = origColombia,
                    RegionId           = regHuila,
                    ProducerId         = prodLuis,
                    VarietalId         = varCaturra,
                    AltitudeId         = alt2100,
                    TastingNotes       = "Lemon Citrus, Honey, Apricot",
                    RoastDate          = DateTime.UtcNow.AddDays(-6),
                    SellerId           = amaraId,
                    Variants           = Variants(8.50m, 15.00m, 27.50m),
                },
            };

            db.Products.AddRange(products);
            await db.SaveChangesAsync();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static List<ProductVariant> Variants(decimal price250g, decimal price500g, decimal price1kg) =>
            new()
            {
                new ProductVariant { Size = "250g", Price = price250g, Quantity = 50 },
                new ProductVariant { Size = "500g", Price = price500g, Quantity = 30 },
                new ProductVariant { Size = "1kg",  Price = price1kg,  Quantity = 15 },
            };
    }
}
