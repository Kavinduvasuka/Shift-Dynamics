using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Infrastructure.Data.Configurations;

public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.ToTable("staff");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EmployeeNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Role).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(x => x.Specialization).HasMaxLength(150);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();

        builder.HasIndex(x => x.EmployeeNumber).IsUnique();
        builder.HasIndex(x => x.UserId).IsUnique();

        // Relationship is configured from User side (User.StaffProfile)
    }
}
