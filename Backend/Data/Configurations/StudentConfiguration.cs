using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EduGame.Entities;

namespace EduGame.Data.Configurations
{
    public class StudentConfiguration : BaseUserConfiguration<Student>
    {
        public override void Configure(EntityTypeBuilder<Student> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Gender)
                .HasColumnOrder(6)
                .IsRequired()
                .HasMaxLength(15);

            builder.Property(x => x.Education)
                .HasColumnOrder(7)
                .IsRequired()
                .HasMaxLength(30);
        }
    }
}