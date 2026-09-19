using ECO.DAL.Entities.Landing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class LandingPageConfiguration : IEntityTypeConfiguration<LandingPage>
{
    public void Configure(EntityTypeBuilder<LandingPage> builder)
    {
        builder.Property(l => l.Title)
            .IsRequired()
            .HasMaxLength(180);

        builder.Property(l => l.Slug)
            .IsRequired()
            .HasMaxLength(180);
        builder.Property(l => l.Template).IsRequired().HasMaxLength(30);
        builder.Property(l => l.AccentColor).IsRequired().HasMaxLength(20);
        builder.Property(l => l.FontFamily).IsRequired().HasMaxLength(40);
        builder.Property(l => l.VideoUrl).HasMaxLength(500);
        builder.Property(l => l.WhatsAppNumber).HasMaxLength(30);
        builder.Property(l => l.WhatsAppMessage).HasMaxLength(300);

        builder.HasIndex(l => l.Slug)
            .IsUnique();

        builder.Property(l => l.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .ValueGeneratedOnAdd();

        builder.HasOne(l => l.Product)
            .WithMany()
            .HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class LandingPageSectionConfiguration : IEntityTypeConfiguration<LandingPageSection>
{
    public void Configure(EntityTypeBuilder<LandingPageSection> builder)
    {
        builder.Property(s => s.SectionType)
            .IsRequired()
            .HasMaxLength(40);

        builder.Property(s => s.Title)
            .HasMaxLength(180);

        builder.Property(s => s.ImageUrl)
            .HasMaxLength(500);

        builder.HasOne(s => s.LandingPage)
            .WithMany(l => l.Sections)
            .HasForeignKey(s => s.LandingPageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.LandingPageId, s.SortOrder });
    }
}
