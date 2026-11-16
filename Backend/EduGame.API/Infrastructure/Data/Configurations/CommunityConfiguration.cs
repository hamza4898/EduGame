using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EduGame.Domain.Entities;
using EduGame.Domain.Enums;

namespace EduGame.Infrastructure.Data.Configurations;

public class CommunityConfiguration : IEntityTypeConfiguration<Community>
{
    public void Configure(EntityTypeBuilder<Community> builder)
    {
        builder.ToTable("Communities");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CommunityId)
            .IsRequired();

        builder.HasIndex(x => x.CommunityId)
            .IsUnique();

        builder.HasIndex(x => x.CommunityName)
            .IsUnique();

        builder.Property(x => x.CommunityName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.AvatarUrl)
            .HasMaxLength(500)
            .IsRequired(false);
        
        builder.HasOne(c => c.Teacher)
            .WithMany()
            .HasForeignKey(c => c.TeacherId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.MembersCount)
            .HasDefaultValue(1);

        builder.Property(x => x.Description)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(x => x.PrivacyType)
            .HasDefaultValue(CommunityPrivacyType.Public)
            .IsRequired();

        builder.Property(x => x.MembersLimit)
            .HasConversion<string>()
            .IsRequired();

        builder.Ignore(x => x.IsFull);
    }
}