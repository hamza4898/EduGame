using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EduGame.Domain.Entities;

namespace EduGame.Infrastructure.Data.Configurations;

public class LessonBlockConfiguration : IEntityTypeConfiguration<LessonBlock>
{
    public void Configure(EntityTypeBuilder<LessonBlock> builder)
    {
        builder.ToTable("LessonBlocks");

        builder.HasKey(lb => lb.Id);

        builder.Property(lb => lb.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(lb => lb.Content).IsRequired();
        
        builder.Property(lb => lb.Order).IsRequired();

        builder.HasOne(lb => lb.Lesson)
            .WithMany(l => l.Blocks)
            .HasForeignKey(lb => lb.LessonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}