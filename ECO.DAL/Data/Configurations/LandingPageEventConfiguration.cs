using ECO.DAL.Entities.Landing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECO.DAL.Data.Configurations;

public class LandingPageEventConfiguration : IEntityTypeConfiguration<LandingPageEvent>
{
    public void Configure(EntityTypeBuilder<LandingPageEvent> builder)
    {
        builder.Property(e => e.SessionId).IsRequired().HasMaxLength(100);
        builder.Property(e => e.EventType).IsRequired().HasMaxLength(30);
        builder.Property(e => e.Source).IsRequired().HasMaxLength(50);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        builder.HasIndex(e => new { e.LandingPageId, e.EventType, e.SessionId })
            .IsUnique()
            .HasFilter("[EventType] = 'visit'");
        builder.HasIndex(e => new { e.LandingPageId, e.CreatedAt });
        builder.HasOne(e => e.LandingPage)
            .WithMany()
            .HasForeignKey(e => e.LandingPageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
