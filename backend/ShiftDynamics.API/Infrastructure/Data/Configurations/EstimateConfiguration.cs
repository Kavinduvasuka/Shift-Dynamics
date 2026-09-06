using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Infrastructure.Data.Configurations;

public class EstimateConfiguration : IEntityTypeConfiguration<Estimate>
{
    public void Configure(EntityTypeBuilder<Estimate> builder)
    {
        builder.ToTable("estimates");

        builder.HasKey(x => x.Id);
        builder.ToTable(t => t.HasCheckConstraint("CK_estimates_non_negative", "\"LaborCost\" >= 0 AND \"PartsCost\" >= 0 AND \"TaxAmount\" >= 0 AND \"DiscountAmount\" >= 0 AND \"Subtotal\" >= 0 AND \"TotalAmount\" >= 0"));

        builder.Property(x => x.EstimateNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.LaborCost)
            .HasPrecision(12, 2);

        builder.Property(x => x.PartsCost)
            .HasPrecision(12, 2);

        builder.Property(x => x.Subtotal)
            .HasPrecision(12, 2);

        builder.Property(x => x.TaxAmount)
            .HasPrecision(12, 2);

        builder.Property(x => x.DiscountAmount)
            .HasPrecision(12, 2);

        builder.Property(x => x.TotalAmount)
            .HasPrecision(12, 2);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.CustomerApproved)
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(x => x.EstimateNumber)
            .IsUnique();

        builder.HasIndex(x => x.WorkOrderId);

        builder.HasOne(x => x.WorkOrder)
            .WithMany()
            .HasForeignKey(x => x.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
