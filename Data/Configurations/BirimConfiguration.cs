using IpGroups.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IpGroups.Data.Configurations
{
    public class BirimConfiguration : BaseEntityConfiguration<Birim>
    {
        public override void Configure(EntityTypeBuilder<Birim> builder)
        {
            base.Configure(builder);

            builder.ToTable("Birimler");

            builder.Property(b => b.Name)
                .IsRequired()
                .HasMaxLength(200);
        }
    }
}
