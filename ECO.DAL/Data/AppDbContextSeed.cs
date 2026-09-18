using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ECO.DAL.Entites.Product;
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
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
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
    }
}
