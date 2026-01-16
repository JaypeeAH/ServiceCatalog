using Microsoft.EntityFrameworkCore;
using ServiceCatalog.Web.Models;

namespace ServiceCatalog.Web.Data
{
    public class ServiceCatalogDbContext : DbContext
    {
        public ServiceCatalogDbContext(DbContextOptions<ServiceCatalogDbContext> options)
            : base(options)
        {
        }

        public DbSet<Capability> Capabilities { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceProcess> ServiceProcesses { get; set; }
        public DbSet<ServiceApplicability> ServiceApplicabilities { get; set; }
        public DbSet<Location> Locations { get; set; }
    
        // ServiceCategories table removed - using Capabilities instead

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Capability -> Service relationship (One-to-Many)
            modelBuilder.Entity<Capability>()
                .HasMany(c => c.Services)
                .WithOne(s => s.Capability)
                .HasForeignKey(s => s.CapabilityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Service -> ServiceProcess relationship (One-to-Many)
            modelBuilder.Entity<Service>()
                .HasMany(s => s.Processes)
                .WithOne(p => p.Service)
                .HasForeignKey(p => p.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ServiceProcess -> ServiceApplicability relationship (One-to-Many)
            // CHANGED: ServiceApplicability now connects to ServiceProcess instead of Service
            modelBuilder.Entity<ServiceProcess>()
                .HasMany(p => p.Applicabilities)
                .WithOne(a => a.ServiceProcess)
                .HasForeignKey(a => a.ServiceProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add indexes for better query performance
            modelBuilder.Entity<Service>()
                .HasIndex(s => s.CapabilityId);

            modelBuilder.Entity<ServiceProcess>()
                .HasIndex(p => p.ServiceId);

            modelBuilder.Entity<ServiceApplicability>()
                .HasIndex(a => a.ServiceProcessId); // Changed from ServiceId

            modelBuilder.Entity<ServiceApplicability>()
                .HasIndex(a => new { a.ServiceProcessId, a.Division }) // Changed from ServiceId
                .IsUnique();

            // Seed initial data - only 8 capabilities from Excel
            SeedCapabilities(modelBuilder);
        }

        private void SeedCapabilities(ModelBuilder modelBuilder)
        {
            // Seed only 8 capabilities matching your Excel file
            modelBuilder.Entity<Capability>().HasData(
                new Capability { Id = 1, Name = "Customer Service", Description = "Customer Service Operations", Order = 1, ColorHex = "#EC5A62" },
                new Capability { Id = 2, Name = "Clinical Services", Description = "Clinical Services Operations", Order = 2, ColorHex = "#89878B" },
                new Capability { Id = 3, Name = "Human Resource Services", Description = "Human Resource Services", Order = 3, ColorHex = "#3B3D40" },
                new Capability { Id = 4, Name = "Supply Chain", Description = "Supply Chain Management", Order = 4, ColorHex = "#FFB900" },
                new Capability { Id = 5, Name = "Information Technology", Description = "Information Technology Services", Order = 5, ColorHex = "#42B3D5" },
                new Capability { Id = 6, Name = "Finance Services", Description = "Finance and Accounting Services", Order = 6, ColorHex = "#69D2B7" },
                new Capability { Id = 7, Name = "Enablement & Governance", Description = "Enablement and Governance", Order = 7, ColorHex = "#B390D4" },
                new Capability { Id = 8, Name = "Procurement Services", Description = "Procurement Services", Order = 8, ColorHex = "#7BB1E8" }
            );
        }
    }
}
