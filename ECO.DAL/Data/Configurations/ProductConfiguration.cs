using ECO.DAL.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .UseIdentityColumn();

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

        builder.Property(p => p.LengthCm).HasColumnType("decimal(10,2)");
        builder.Property(p => p.WidthCm).HasColumnType("decimal(10,2)");
        builder.Property(p => p.HeightCm).HasColumnType("decimal(10,2)");
        builder.Property(p => p.WeightKg).HasColumnType("decimal(10,3)");
    }
}
