
using ECO.DAL.Entities.OrderEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECO.DAL.Data.Configurations
{
    internal class DeliveryMethodConfiguration : IEntityTypeConfiguration<DeliveryMethod>
    {
        public void Configure(EntityTypeBuilder<DeliveryMethod> builder)
        {
            builder.Property(o => o.Price).HasColumnType("decimal(18,2)");
            builder.HasData(
                new DeliveryMethod { Id = 1, DeliveryTime = "1-2 business days", Description = "Express delivery", Name = "DHL Express", Price = 30, LogoUrl = "https://cdn.simpleicons.org/dhl/FFCC00" },
                new DeliveryMethod { Id = 2, DeliveryTime = "2-3 business days", Description = "Reliable priority delivery", Name = "FedEx", Price = 20, LogoUrl = "https://cdn.simpleicons.org/fedex/4D148C" },
                new DeliveryMethod { Id = 3, DeliveryTime = "3-5 business days", Description = "Fast local delivery", Name = "Aramex", Price = 15, LogoUrl = "https://cdn.simpleicons.org/aramex/D71920" },
                new DeliveryMethod { Id = 4, DeliveryTime = "4-6 business days", Description = "Affordable doorstep delivery", Name = "Bosta", Price = 10, LogoUrl = "https://cdn.simpleicons.org/bosta/111827" },
                new DeliveryMethod { Id = 5, DeliveryTime = "5-7 business days", Description = "Economy delivery", Name = "Egypt Post", Price = 5, LogoUrl = "https://cdn.simpleicons.org/egyptpost/0B5FA5" }
            );
        }
    }
}
