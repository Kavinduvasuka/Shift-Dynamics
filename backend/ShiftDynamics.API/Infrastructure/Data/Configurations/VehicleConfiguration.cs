using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Infrastructure.Data.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RegistrationNumber)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.Make)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Model)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Year)
            .IsRequired();

        builder.Property(x => x.VIN)
            .HasMaxLength(50);

        builder.Property(x => x.Color)
            .HasMaxLength(50);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.RegistrationNumber)
            .IsUnique();

        builder.HasIndex(x => x.VIN)
            .IsUnique();

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.Vehicles)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
