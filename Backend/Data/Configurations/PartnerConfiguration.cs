using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EduGame.Entities;

namespace EduGame.Data.Configurations
{
    public class PartnerConfiguration : BaseUserConfiguration<Partner>
    {
        public override void Configure(EntityTypeBuilder<Partner> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Organization)
                .HasColumnOrder(6)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.TypeOfCooperation)
                .HasColumnOrder(7)
                .IsRequired()
                .HasMaxLength(30);
        }
    }
}