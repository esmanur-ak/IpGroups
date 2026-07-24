using IpGroups.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IpGroups.Data.Configurations
{
    public class PersonConfiguration : BaseEntityConfiguration<Person>
    {
        public override void Configure(EntityTypeBuilder<Person> builder)
        {
            base.Configure(builder);

            builder.ToTable("Personeller");

            builder.Property(p => p.Ad)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Soyad)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.Domain)
                .IsRequired()
                .HasMaxLength(200);

            // İlişkiler
            builder.HasOne(p => p.Bina)
                .WithMany()
                .HasForeignKey(p => p.BinaId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(p => p.Birim)
                .WithMany()
                .HasForeignKey(p => p.BirimId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(p => p.IpAddress)
                .WithMany()
                .HasForeignKey(p => p.IpAddressId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
