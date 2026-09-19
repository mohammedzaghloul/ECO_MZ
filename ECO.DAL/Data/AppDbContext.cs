using ECO.DAL.Entities;
using ECO.DAL.Entities.Landing;
using ECO.DAL.Entities.OrderEntities;
using ECO.DAL.Entities.Product;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.DAL.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Photo> Photos { get; set; }
        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderItem> OrderItems { get; set; }
        public virtual DbSet<DeliveryMethod>  DeliveryMethods { get; set; }
        public virtual DbSet<Review> Reviews { get; set; }
        public virtual DbSet<WishlistItem> WishlistItems { get; set; }
        public virtual DbSet<ProductSpecItem> ProductSpecifications { get; set; }
        public virtual DbSet<LandingPage> LandingPages { get; set; }
        public virtual DbSet<LandingPageSection> LandingPageSections { get; set; }
        public virtual DbSet<Discount> Discounts { get; set; }
        public virtual DbSet<LocationCatalog> LocationCatalogs { get; set; }
        public virtual DbSet<LocationGovernorate> LocationGovernorates { get; set; }
        public virtual DbSet<LocationCity> LocationCities { get; set; }
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<LandingPageEvent> LandingPageEvents { get; set; }
        public virtual DbSet<StoreSettings> StoreSettings { get; set; }
        public virtual DbSet<EmailTemplateSetting> EmailTemplateSettings { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<LocationCity>()
                .Property(city => city.ShippingPrice)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<EmailTemplateSetting>(entity =>
            {
                entity.HasIndex(x => new { x.OwnerEmail, x.TemplateType, x.Language })
                    .HasDatabaseName("IX_EmailTemplateSettings_Owner_Type_Lang");
                entity.HasIndex(x => new { x.TemplateType, x.Language })
                    .HasDatabaseName("IX_EmailTemplateSettings_Type_Lang");
            });

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);    
        }
    }
}
