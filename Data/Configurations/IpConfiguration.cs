using IpGroups.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IpGroups.Data.Configurations
{
    public class IpConfiguration : BaseEntityConfiguration<Ip>
    {
        public override void Configure(EntityTypeBuilder<Ip> builder)
        {
            base.Configure(builder);

            builder.ToTable("Ipler");

            builder.Property(i => i.IpNo)
                .IsRequired()
                .HasMaxLength(45); // IPv6 max uzunluğu

            builder.Property(i => i.Description)
                .HasMaxLength(500);
        }
    }
}
