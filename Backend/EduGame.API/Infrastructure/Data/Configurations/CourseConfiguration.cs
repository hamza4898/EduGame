using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EduGame.Domain.Entities;

namespace EduGame.Infrastructure.Data.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(2000);

        builder.Property(c => c.PriceCoins).HasDefaultValue(0);

        builder.Property(c => c.Difficulty)
            .HasConversion<int>()
            .IsRequired();

        builder.HasOne(c => c.Community)
            .WithMany() 
            .HasForeignKey(c => c.CommunityId)
            .OnDelete(DeleteBehavior.Cascade); 

        builder.HasOne(c => c.SkillSubSection)
            .WithMany()
            .HasForeignKey(c => c.SkillSubSectionId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(c => c.Sections)
            .WithOne(s => s.Course)
            .HasForeignKey(s => s.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t => t.HasCheckConstraint("CK_Course_PriceCoins_NonNegative", "\"PriceCoins\" >= 0"));
    }
}