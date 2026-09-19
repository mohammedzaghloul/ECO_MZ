
using ECO.DAL.Entities.OrderEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.DAL.Data.Configurations
{
    internal class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        void IEntityTypeConfiguration<Order>.Configure(EntityTypeBuilder<Order> builder)
        {
            builder.OwnsOne(o => o.ShippingAddress, n => n.WithOwner());
            builder.HasMany(o=>o.OrderItems).WithOne().OnDelete(DeleteBehavior.Cascade);
            builder.Property(o=>o.Status).HasConversion(o=>o.ToString(),o=>(Status)
            Enum.Parse(typeof(Status),o));

            builder.Property(o=>o.SubTotal).HasColumnType("decimal(18,2)");
            builder.Property(o=>o.ShippingPrice).HasColumnType("decimal(18,2)");
            builder.Property(o=>o.Discount).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
            builder.Property(o=>o.PaymentMethod).HasMaxLength(20).HasDefaultValue("Stripe");
            builder.Property(o => o.BuyerPhone).HasMaxLength(30);
            builder.HasOne(o => o.LandingPage)
                .WithMany()
                .HasForeignKey(o => o.LandingPageId)
                .OnDelete(DeleteBehavior.SetNull);
            builder.HasIndex(o => new { o.BasketId, o.BuyerEmail })
                .IsUnique()
                .HasFilter("[BasketId] IS NOT NULL AND [BuyerEmail] IS NOT NULL");

        }
    }
}
