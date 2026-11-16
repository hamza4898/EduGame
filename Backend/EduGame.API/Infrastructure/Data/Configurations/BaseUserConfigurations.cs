using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EduGame.Domain.Entities;

namespace EduGame.Infrastructure.Data.Configurations;

public class BaseUserConfiguration<T> : IEntityTypeConfiguration<T>
    where T : BaseUser
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.Property(x => x.Id)
            .HasColumnOrder(0)
            .HasMaxLength(450);

        builder.Property(x => x.FirstName)
            .HasColumnOrder(2)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.LastName)
            .HasColumnOrder(3)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.DateOfBirth)
            .HasColumnOrder(4)
            .HasColumnType("date")
            .IsRequired();

        if (typeof(T) == typeof(BaseUser))
        {
            builder.HasKey(x => x.Id);
            builder.ToTable("BaseUsers");
        }

        builder.Property(x => x.CoinsBalance)
            .HasDefaultValue(0)
            .IsRequired();

        builder.ToTable(t => t.HasCheckConstraint("CK_User_StarsBalance_NonNegative", "\"CoinsBalance\" >= 0"));
    }
}