using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EduGame.Domain.Entities;

namespace EduGame.Infrastructure.Data.Configurations;

public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("Lessons");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.XpReward)
            .HasDefaultValue(0)
            .IsRequired();
        
        builder.ToTable(t => t.HasCheckConstraint("CK_Lesson_XpReward_NonNegative", "\"XpReward\" >= 0"));

        builder.Property(l => l.Title).HasMaxLength(250).IsRequired();
    }
}