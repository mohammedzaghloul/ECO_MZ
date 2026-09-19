using ECO.DAL.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProductSpecificationConfiguration : IEntityTypeConfiguration<ProductSpecItem>
{
    public void Configure(EntityTypeBuilder<ProductSpecItem> builder)
    {
        builder.HasKey(specification => specification.Id);
        builder.Property(specification => specification.Label).IsRequired().HasMaxLength(100);
        builder.Property(specification => specification.Value).IsRequired().HasMaxLength(250);
        builder.HasOne(specification => specification.Product)
            .WithMany(product => product.Specifications)
            .HasForeignKey(specification => specification.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(specification => new { specification.ProductId, specification.SortOrder });
    }
}
