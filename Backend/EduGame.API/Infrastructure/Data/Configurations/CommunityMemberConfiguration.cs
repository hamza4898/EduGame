using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EduGame.Domain.Entities;
using EduGame.Domain.Enums;

namespace EduGame.Infrastructure.Data.Configurations;

public class CommunityMemberConfiguration : IEntityTypeConfiguration<CommunityMember>
{
    public void Configure(EntityTypeBuilder<CommunityMember> builder)
    {
        builder.ToTable("CommunityMembers");

        builder.HasKey(x => new { x.CommunityId, x.UserId });

        builder.Property(x => x.UserId)
            .HasMaxLength(450)
            .IsRequired();
        
        builder.Property(x => x.Role)
            .HasConversion(
                v => v.ToString(),
                v => (ClanRole)Enum.Parse(typeof(ClanRole), v))
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.JoinedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

        builder.HasOne(x => x.Community)
            .WithMany(c => c.Members)
            .HasForeignKey(x => x.CommunityId)
            .HasPrincipalKey(c => c.CommunityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}