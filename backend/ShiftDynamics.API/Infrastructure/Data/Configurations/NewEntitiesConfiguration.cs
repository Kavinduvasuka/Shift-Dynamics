using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Infrastructure.Data.Configurations;

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("password_reset_tokens");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
        builder.HasIndex(x => x.TokenHash);
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class WorkshopBayConfiguration : IEntityTypeConfiguration<WorkshopBay>
{
    public void Configure(EntityTypeBuilder<WorkshopBay> builder)
    {
        builder.ToTable("workshop_bays");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
    }
}

public class JobAssignmentConfiguration : IEntityTypeConfiguration<JobAssignment>
{
    public void Configure(EntityTypeBuilder<JobAssignment> builder)
    {
        builder.ToTable("job_assignments");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.WorkOrder).WithMany().HasForeignKey(x => x.WorkOrderId);
        builder.HasOne(x => x.Mechanic).WithMany().HasForeignKey(x => x.MechanicStaffId);
        builder.HasOne(x => x.Bay).WithMany().HasForeignKey(x => x.BayId).OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => new { x.MechanicStaffId, x.IsActive });
        builder.HasIndex(x => new { x.BayId, x.IsActive });
    }
}

public class LaborSessionConfiguration : IEntityTypeConfiguration<LaborSession>
{
    public void Configure(EntityTypeBuilder<LaborSession> builder)
    {
        builder.ToTable("labor_sessions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.HasOne(x => x.WorkOrder).WithMany().HasForeignKey(x => x.WorkOrderId);
        builder.HasOne(x => x.Mechanic).WithMany().HasForeignKey(x => x.MechanicStaffId);
    }
}

public class PartConfiguration : IEntityTypeConfiguration<Part>
{
    public void Configure(EntityTypeBuilder<Part> builder)
    {
        builder.ToTable("parts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PartNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.PartNumber).IsUnique();
        builder.HasOne(x => x.Inventory).WithOne(i => i.Part).HasForeignKey<InventoryItem>(i => i.PartId);
    }
}

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("inventory_items");
        builder.HasKey(x => x.Id);
        builder.ToTable(t => t.HasCheckConstraint("CK_inventory_items_non_negative", "\"OnHandQty\" >= 0 AND \"ReservedQty\" >= 0 AND \"ReorderLevel\" >= 0 AND \"UnitCost\" >= 0"));
        builder.Property(x => x.UnitCost).HasPrecision(18, 2);
        builder.HasIndex(x => x.PartId).IsUnique();
    }
}

public class PartRequisitionConfiguration : IEntityTypeConfiguration<PartRequisition>
{
    public void Configure(EntityTypeBuilder<PartRequisition> builder)
    {
        builder.ToTable("part_requisitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PartSpec).HasMaxLength(500);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Urgency).HasConversion<string>().HasMaxLength(30);
        builder.HasOne(x => x.WorkOrder).WithMany().HasForeignKey(x => x.WorkOrderId);
        builder.HasOne(x => x.RequestedBy).WithMany().HasForeignKey(x => x.RequestedByStaffId);
        builder.HasOne(x => x.Part).WithMany().HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("stock_movements");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(30);
        builder.HasOne(x => x.Part).WithMany().HasForeignKey(x => x.PartId);
    }
}

public class ContactInquiryConfiguration : IEntityTypeConfiguration<ContactInquiry>
{
    public void Configure(EntityTypeBuilder<ContactInquiry> builder)
    {
        builder.ToTable("contact_inquiries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Subject).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
    }
}

public class EmergencyServiceProviderConfiguration : IEntityTypeConfiguration<EmergencyServiceProvider>
{
    public void Configure(EntityTypeBuilder<EmergencyServiceProvider> builder)
    {
        builder.ToTable("emergency_service_providers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Latitude).HasPrecision(9, 6);
        builder.Property(x => x.Longitude).HasPrecision(9, 6);
    }
}

public class VendorRegistrationConfiguration : IEntityTypeConfiguration<VendorRegistration>
{
    public void Configure(EntityTypeBuilder<VendorRegistration> builder)
    {
        builder.ToTable("vendor_registrations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BusinessName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(x => x.Email);
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.UserId, x.IsRead });
    }
}

public class VendorProfileConfiguration : IEntityTypeConfiguration<VendorProfile>
{
    public void Configure(EntityTypeBuilder<VendorProfile> builder)
    {
        builder.ToTable("vendor_profiles");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BusinessName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ContactPerson).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Mobile).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.Specialization).HasMaxLength(200);
        builder.Property(x => x.ApprovalStatus).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(x => x.UserId).IsUnique();
        builder.HasIndex(x => x.Email);

        builder.HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<VendorProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Registration)
            .WithMany()
            .HasForeignKey(x => x.RegistrationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
