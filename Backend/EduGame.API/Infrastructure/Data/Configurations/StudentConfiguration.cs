using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EduGame.Domain.Entities;

namespace EduGame.Infrastructure.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {   
        builder.Property(s => s.GlobalXp)
            .HasDefaultValue(0)
            .IsRequired();  

        builder.HasIndex(s => s.GlobalXp)
            .HasDatabaseName("IX_Students_GlobalXp");     
    }
}