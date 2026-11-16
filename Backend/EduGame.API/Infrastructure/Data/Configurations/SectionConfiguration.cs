using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EduGame.Domain.Entities;

namespace EduGame.Infrastructure.Data.Configurations;

public class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> builder)
    {
        builder.ToTable("Sections");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title).HasMaxLength(250).IsRequired();
        builder.Property(s => s.Order).IsRequired();

        builder.HasMany(s => s.Lessons)
            .WithOne(l => l.Section)
            .HasForeignKey(l => l.SectionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}