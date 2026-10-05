using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Infrastructure.Data.Configurations;

public class EmergencyRequestConfiguration : IEntityTypeConfiguration<EmergencyRequest>
{
    public void Configure(EntityTypeBuilder<EmergencyRequest> builder)
    {
        builder.ToTable("emergency_requests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Location)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.ProblemDescription)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.Latitude)
            .HasPrecision(10, 7);

        builder.Property(x => x.Longitude)
            .HasPrecision(10, 7);

        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.VehicleId);
        builder.HasIndex(x => x.AssignedStaffId);
        builder.HasIndex(x => x.Status);

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Vehicle)
            .WithMany()
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.AssignedStaff)
            .WithMany()
            .HasForeignKey(x => x.AssignedStaffId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
