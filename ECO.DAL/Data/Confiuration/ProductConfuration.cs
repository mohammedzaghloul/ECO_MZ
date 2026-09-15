using ECO.DAL.Entites.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProductConfuration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.NewPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.OldPrice)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.CategoryId)
            .IsRequired();

        builder.HasData(
            new Product
            {
                Id = 1,
                Name = "Product 1",
                Description = "Description for Product 1",
                NewPrice = 10.99m,
                OldPrice = 15.99m,
                CategoryId = 1
            },
            new Product
            {
                Id = 2,
                Name = "Product 2",
                Description = "Description for Product 2",
                NewPrice = 19.99m,
                OldPrice = 25.99m,
                CategoryId = 1
            }
        );
    }
}