using IpGroups.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IpGroups.Data.Configurations
{
    public class BinaConfiguration : BaseEntityConfiguration<Bina>
    {
        public override void Configure(EntityTypeBuilder<Bina> builder)
        {
            base.Configure(builder);

            builder.ToTable("Binalar");

            builder.Property(b => b.Name)
                .IsRequired() 
                .HasMaxLength(200);
        }
    }
}
