using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");

        builder.HasKey(x => x.Id);
        builder.ToTable(t => t.HasCheckConstraint("CK_invoices_non_negative", "\"LaborCost\" >= 0 AND \"PartsCost\" >= 0 AND \"TaxAmount\" >= 0 AND \"DiscountAmount\" >= 0 AND \"Subtotal\" >= 0 AND \"TotalAmount\" >= 0 AND \"AmountPaid\" >= 0 AND \"BalanceDue\" >= 0 AND \"AmountPaid\" <= \"TotalAmount\""));

        builder.Property(x => x.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.LaborCost).HasPrecision(12, 2);
        builder.Property(x => x.PartsCost).HasPrecision(12, 2);
        builder.Property(x => x.Subtotal).HasPrecision(12, 2);
        builder.Property(x => x.TaxAmount).HasPrecision(12, 2);
        builder.Property(x => x.DiscountAmount).HasPrecision(12, 2);
        builder.Property(x => x.TotalAmount).HasPrecision(12, 2);
        builder.Property(x => x.AmountPaid).HasPrecision(12, 2);
        builder.Property(x => x.BalanceDue).HasPrecision(12, 2);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(x => x.InvoiceNumber)
            .IsUnique();

        builder.HasIndex(x => x.WorkOrderId);

        builder.HasOne(x => x.WorkOrder)
            .WithMany()
            .HasForeignKey(x => x.WorkOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Estimate)
            .WithMany()
            .HasForeignKey(x => x.EstimateId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
