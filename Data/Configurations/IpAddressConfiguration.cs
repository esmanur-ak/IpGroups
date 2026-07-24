using IpGroups.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IpGroups.Data.Configurations
{
    public class IpAddressConfiguration : BaseEntityConfiguration<IpAddress>
    {
        public override void Configure(EntityTypeBuilder<IpAddress> builder)
        {
            base.Configure(builder);

            builder.ToTable("IpAdresler");

            builder.Property(ia => ia.FullIpAddress)
                .IsRequired()
                .HasMaxLength(45);

            builder.Property(ia => ia.Octet)
                .IsRequired();

            builder.Property(ia => ia.IsAssigned)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasOne(ia => ia.IpGroup)
                .WithMany(ip => ip.IpAddresses)
                .HasForeignKey(ia => ia.IpGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Aynı grupta aynı oktet tekrar edemez (aktif kayıtlar için)
            builder.HasIndex(ia => new { ia.IpGroupId, ia.Octet })
                .HasFilter("\"IsDeleted\" = false")
                .IsUnique();

            // Full IP adresi benzersiz olmalı (aktif kayıtlar için)
            builder.HasIndex(ia => ia.FullIpAddress)
                .HasFilter("\"IsDeleted\" = false")
                .IsUnique();
        }
    }
}
