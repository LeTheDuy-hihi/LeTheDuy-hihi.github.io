using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutoGarageManager.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Part> Parts { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceHistory> ServiceHistories { get; set; }
        public DbSet<ServiceHistoryService> ServiceHistoryServices { get; set; }
        public DbSet<ServiceHistoryPart> ServiceHistoryParts { get; set; }
        public DbSet<MaintenanceReminder> MaintenanceReminders { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Customer)
                .WithMany(c => c.Vehicles)
                .HasForeignKey(v => v.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServiceHistory>()
                .HasOne(sh => sh.Vehicle)
                .WithMany(v => v.ServiceHistories)
                .HasForeignKey(sh => sh.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServiceHistory>()
                .HasOne(sh => sh.Mechanic)
                .WithMany()
                .HasForeignKey(sh => sh.MechanicId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ServiceHistoryService>()
                .HasOne(shs => shs.ServiceHistory)
                .WithMany(sh => sh.ServicesPerformed)
                .HasForeignKey(shs => shs.ServiceHistoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServiceHistoryService>()
                .HasOne(shs => shs.Service)
                .WithMany(s => s.ServiceHistories)
                .HasForeignKey(shs => shs.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ServiceHistoryPart>()
                .HasOne(shp => shp.ServiceHistory)
                .WithMany(sh => sh.PartsUsed)
                .HasForeignKey(shp => shp.ServiceHistoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServiceHistoryPart>()
                .HasOne(shp => shp.Part)
                .WithMany(p => p.ServiceHistories)
                .HasForeignKey(shp => shp.PartId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed data
            modelBuilder.Entity<Customer>().HasData(
                new Customer { Id = 1, Name = "John Doe", FullName = "John Doe", Email = "john@example.com", Phone = "123-456-7890", Address = "123 Main St", CreatedAt = new DateTime(2024, 1, 1) },
                new Customer { Id = 2, Name = "Jane Smith", FullName = "Jane Smith", Email = "jane@example.com", Phone = "987-654-3210", Address = "456 Oak Ave", CreatedAt = new DateTime(2024, 1, 1) }
            );

            modelBuilder.Entity<Vehicle>().HasData(
                new Vehicle { Id = 1, CustomerId = 1, Make = "Toyota", Model = "Camry", Year = 2020, LicensePlate = "ABC123", Mileage = 15000, CreatedAt = new DateTime(2024, 1, 1) },
                new Vehicle { Id = 2, CustomerId = 2, Make = "Honda", Model = "Civic", Year = 2019, LicensePlate = "XYZ789", Mileage = 20000, CreatedAt = new DateTime(2024, 1, 1) }
            );

            modelBuilder.Entity<Part>().HasData(
                new Part { Id = 1, Name = "Oil Filter", Description = "Engine oil filter", Price = 15.99m, Quantity = 50, Category = "Filters", Supplier = "ACME Parts" },
                new Part { Id = 2, Name = "Brake Pads", Description = "Front brake pads", Price = 89.99m, Quantity = 30, Category = "Brakes", Supplier = "ACME Parts" },
                new Part { Id = 3, Name = "Air Filter", Description = "Engine air filter", Price = 24.99m, Quantity = 40, Category = "Filters", Supplier = "ACME Parts" }
            );

            modelBuilder.Entity<Service>().HasData(
                new Service { Id = 1, Name = "Oil Change", Description = "Complete oil change service", Price = 49.99m, EstimatedDuration = 30 },
                new Service { Id = 2, Name = "Brake Service", Description = "Brake inspection and replacement", Price = 149.99m, EstimatedDuration = 60 },
                new Service { Id = 3, Name = "Tire Rotation", Description = "Tire rotation and balance", Price = 29.99m, EstimatedDuration = 45 }
            );
        }
    }

    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
    }
}