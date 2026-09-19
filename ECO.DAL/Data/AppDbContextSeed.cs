using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ECO.DAL.Entities.Product;
using ECO.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECO.DAL.Data
{
    public class AppDbContextSeed
    {
        public static async Task SeedAsync(AppDbContext context, ILoggerFactory? loggerFactory = null)
        {
            var logger = loggerFactory?.CreateLogger<AppDbContextSeed>();

            try
            {
                var seedPath = GetSeedDataPath();

                if (!await context.LocationCatalogs.AnyAsync(c => c.Key == "egypt"))
                {
                    var locations = new LocationCatalog
                    {
                        Key = "egypt",
                        JsonContent = await File.ReadAllTextAsync(Path.Combine(seedPath, "locations-egypt.json"))
                    };
                    await context.LocationCatalogs.AddAsync(locations);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Location catalog seeded successfully.");
                }

                if (!await context.LocationGovernorates.AnyAsync())
                {
                    var locationsData = await File.ReadAllTextAsync(Path.Combine(seedPath, "locations-egypt.json"));
                    var locationRows = JsonSerializer.Deserialize<List<SeedGovernorate>>(locationsData,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
                    foreach (var row in locationRows)
                    {
                        var governorate = new LocationGovernorate { Value = row.Value, En = row.En, Ar = row.Ar };
                        governorate.Cities = row.Cities.Select(city => new LocationCity
                        {
                            Value = city.Value, En = city.En, Ar = city.Ar,
                            ShippingPrice = city.ShippingPrice,
                            DeliveryDays = city.DeliveryDays,
                            ShippingAvailable = city.ShippingAvailable
                        }).ToList();
                        context.LocationGovernorates.Add(governorate);
                    }
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Normalized locations seeded successfully.");
                }

                // 1. Seed Categories
                if (!await context.Categories.AnyAsync())
                {
                    var categoriesData = await File.ReadAllTextAsync(Path.Combine(seedPath, "categories.json"));
                    var categories = JsonSerializer.Deserialize<List<Category>>(categoriesData, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (categories != null && categories.Count > 0)
                    {
                        await context.Categories.AddRangeAsync(categories);
                        await context.SaveChangesAsync();
                        logger?.LogInformation("Categories seeded successfully.");
                    }
                }

                // 2. Seed Products
                var productCount = await context.Products.CountAsync();
                if (productCount < 50)
                {
                    var productsData = await File.ReadAllTextAsync(Path.Combine(seedPath, "products.json"));
                    var allProducts = JsonSerializer.Deserialize<List<Product>>(productsData, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (allProducts != null && allProducts.Count > 0)
                    {
                        var existingProductNames = await context.Products
                            .AsNoTracking()
                            .Select(p => p.Name.Trim())
                            .ToListAsync();

                        var productsToAdd = allProducts
                            .Where(p => !existingProductNames.Any(existing => string.Equals(existing, p.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
                            .ToList();

                        if (productsToAdd.Count > 0)
                        {
                            await context.Products.AddRangeAsync(productsToAdd);
                            await context.SaveChangesAsync();
                        }

                        logger?.LogInformation("Products ensured to the minimum seed count successfully.");
                    }
                }

                // 3. Seed Photos
                var photoCount = await context.Photos.CountAsync();
                if (photoCount < 50)
                {
                    var photosData = await File.ReadAllTextAsync(Path.Combine(seedPath, "photos.json"));
                    var photos = JsonSerializer.Deserialize<List<Photo>>(photosData, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (photos != null && photos.Count > 0)
                    {
                        var existingPhotoNames = await context.Photos
                            .AsNoTracking()
                            .Select(p => p.Name.Trim())
                            .ToListAsync();

                        var photosToAdd = photos
                            .Where(p => !existingPhotoNames.Any(existing => string.Equals(existing, p.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
                            .ToList();

                        if (photosToAdd.Count > 0)
                        {
                            await context.Photos.AddRangeAsync(photosToAdd);
                            await context.SaveChangesAsync();
                        }

                        logger?.LogInformation("Photos ensured to the minimum seed count successfully.");
                    }
                }

                await SeedReviewsAsync(context, logger);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        private static async Task SeedReviewsAsync(AppDbContext context, ILogger? logger)
            {
                if (await context.Reviews.AnyAsync())
                    return;

                var userIds = await context.Users
                    .AsNoTracking()
                    .Select(user => user.Id)
                    .Take(3)
                    .ToListAsync();
                var productIds = await context.Products
                    .AsNoTracking()
                    .OrderBy(product => product.Id)
                    .Take(12)
                    .Select(product => product.Id)
                    .ToListAsync();

                if (userIds.Count == 0 || productIds.Count == 0)
                {
                    logger?.LogInformation("Review seed skipped because users or products are not available yet.");
                    return;
                }

                var comments = new[]
                {
                    ("Great quality", "Exactly as described and works perfectly."),
                    ("Very happy", "Good value and fast delivery."),
                    ("Recommended", "The product feels well made and reliable.")
                };

                var reviews = new List<Review>();
                for (var productIndex = 0; productIndex < productIds.Count; productIndex++)
                {
                    for (var userIndex = 0; userIndex < userIds.Count; userIndex++)
                    {
                        var comment = comments[userIndex];
                        reviews.Add(new Review
                        {
                            ProductId = productIds[productIndex],
                            UserId = userIds[userIndex],
                            Rating = 4 + ((productIndex + userIndex) % 2),
                            Title = comment.Item1,
                            Comment = comment.Item2,
                            IsVerifiedPurchase = false,
                            CreatedAt = DateTime.UtcNow.AddDays(-(productIndex + userIndex + 1))
                        });
                    }
                }

                await context.Reviews.AddRangeAsync(reviews);
                await context.SaveChangesAsync();
                logger?.LogInformation("Seeded {ReviewCount} development product reviews.", reviews.Count);
        }

        private static string GetSeedDataPath()
        {
            var baseDir = AppContext.BaseDirectory;
            var pathInOutput = Path.Combine(baseDir, "Data", "SeedData");
            if (Directory.Exists(pathInOutput))
                return pathInOutput;

            var relativePath = Path.Combine(baseDir, "..", "..", "..", "..", "ECO.DAL", "Data", "SeedData");
            if (Directory.Exists(relativePath))
                return Path.GetFullPath(relativePath);

            var directDalPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "ECO.DAL", "Data", "SeedData");
            if (Directory.Exists(directDalPath))
                return Path.GetFullPath(directDalPath);

            var currentDirSeedPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "SeedData");
            if (Directory.Exists(currentDirSeedPath))
                return Path.GetFullPath(currentDirSeedPath);

            return pathInOutput;
        }

        private sealed class SeedGovernorate
        {
            public string Value { get; set; } = string.Empty;
            public string En { get; set; } = string.Empty;
            public string Ar { get; set; } = string.Empty;
            public List<SeedCity> Cities { get; set; } = [];
        }

        private sealed class SeedCity
        {
            public string Value { get; set; } = string.Empty;
            public string En { get; set; } = string.Empty;
            public string Ar { get; set; } = string.Empty;
            public decimal ShippingPrice { get; set; }
            public int DeliveryDays { get; set; }
            public bool ShippingAvailable { get; set; } = true;
        }
    }
}
