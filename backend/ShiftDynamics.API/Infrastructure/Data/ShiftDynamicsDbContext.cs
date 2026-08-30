using Microsoft.EntityFrameworkCore;
using ShiftDynamics.API.Domain.Entities;

namespace ShiftDynamics.API.Infrastructure.Data;

public class ShiftDynamicsDbContext : DbContext
{
    public ShiftDynamicsDbContext(DbContextOptions<ShiftDynamicsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShiftDynamicsDbContext).Assembly);
    }
}
