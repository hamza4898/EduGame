using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EduGame.Entities;

namespace EduGame.Data.Configurations
{
    public class TeacherConfiguration : BaseConfiguration<Teacher>
    {
        public override void Configure(EntityTypeBuilder<Teacher> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Gender)
                .HasColumnOrder(6)
                .IsRequired()
                .HasMaxLength(15);

            builder.Property(x => x.Subject)
                .HasColumnOrder(7)
                .IsRequired()
                .HasMaxLength(20);
        }
    }
}