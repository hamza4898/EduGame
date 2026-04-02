using EduGame.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduGame.Data.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.HashedToken)
                .IsUnique()
                .HasDatabaseName("IX_RefreshToken_HashedToken");

            builder.HasIndex(x => x.UserId);

            builder.Property(x => x.HashedToken)
                .IsRequired()
                .HasMaxLength(256)
                .IsUnicode(false);

            builder.Property(x => x.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.CreatedAt).IsRequired();

            builder.Property(x => x.ExpiresAt).IsRequired();

            builder.Ignore(x => x.IsActive);
        }
    }
}