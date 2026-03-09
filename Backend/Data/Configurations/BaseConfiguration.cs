using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EduGame.Entities;

namespace EduGame.Data.Configurations
{
    public abstract class BaseConfiguration<T> : IEntityTypeConfiguration<T>
        where T: BaseUser
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            builder.Property<int>("Id")
                .HasColumnOrder(0)
                .ValueGeneratedOnAdd()
                .HasComment($"Primary key (auto increment) of {typeof(T).Name} for database");

            builder.Property(x => x.ExternalId)
                .HasColumnOrder(1)
                .HasMaxLength(36)
                .IsRequired()
                .HasComment($"Immutable unique business key for {typeof(T).Name}");
            
            builder.HasKey("Id");

            builder.HasAlternateKey(x => x.ExternalId);  

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
                .IsRequired()
                .HasPrecision(0);

            builder.Property(x => x.Motivation)
                .HasColumnOrder(5)
                .HasColumnType("varchar(200)")
                .IsRequired(false)
                .HasComment("Optional cover letter text from the registration form");
        }
    }
}