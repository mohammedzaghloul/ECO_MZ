using ECO.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DiscountConfiguration : IEntityTypeConfiguration<Discount>
{
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        builder.Property(d => d.Code)
            .IsRequired()
            .HasMaxLength(40);

        builder.HasIndex(d => d.Code)
            .IsUnique();

        builder.Property(d => d.Value)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
    }
}
