using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EduGame.Domain.Entities;

namespace EduGame.Infrastructure.Data.Configurations;

public class UserSubSectionXpConfiguration : IEntityTypeConfiguration<UserSubSectionXp>
{
    public void Configure(EntityTypeBuilder<UserSubSectionXp> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SkillSubSection)
            .WithMany() 
            .HasForeignKey(x => x.SkillSubSectionId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(x => new { x.UserId, x.SkillSubSectionId }).IsUnique();
    }
}