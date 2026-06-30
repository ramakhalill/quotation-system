using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static AuroraManagementAPI.Models.Project;

namespace AuroraManagementAPI.Models
{
    public class AuroraDbContext : IdentityDbContext<IdentityUser>
    {
        public AuroraDbContext(DbContextOptions<AuroraDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Client> Clients { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<QuoteItem> QuoteItems { get; set; }
        public DbSet<MissingDevice> MissingDevices { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<SupplierDevice> SupplierDevices { get; set; }
        public DbSet<SystemType> SystemTypes { get; set; }
        public DbSet<ProjectSystemType> ProjectSystemTypes { get; set; }


        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // قبل الحفظ
            foreach (var entry in ChangeTracker.Entries<Device>())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    var device = entry.Entity;
                    // إذا ActualPrice موجود، احسب SalesPrice تلقائياً
                    if ((device.SystemType != "Low Current" && device.SystemType != "Smart Wi-Fi") && device.ActualPrice > 0)
                    {
                        device.SalesPrice = Math.Round(device.ActualPrice * 1.3m, 2); // +30% مع تقريب لرقمين عشريين
                    }
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Table mappings
            modelBuilder.Entity<Client>().ToTable("Clients");
            modelBuilder.Entity<Project>().ToTable("Projects");
            modelBuilder.Entity<Device>().ToTable("Devices");
            modelBuilder.Entity<Quote>().ToTable("Quotes");
            modelBuilder.Entity<QuoteItem>().ToTable("Quote_Items");
            modelBuilder.Entity<MissingDevice>().ToTable("MissingDevices");
            modelBuilder.Ignore<User>();

            // Relationships
            modelBuilder.Entity<Client>()
                .HasOne(c => c.CreatedByUser)
                .WithMany() // since User doesn’t have Clients collection
                .HasForeignKey(c => c.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Project>()
                .HasOne(p => p.Client)
                .WithMany(c => c.Projects)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Quote>()
                .HasOne(q => q.Client)
                .WithMany(c => c.Quotes)
                .HasForeignKey(q => q.ClientId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Quote>()
                .HasOne(q => q.Project)
                .WithMany(p => p.Quotes)
                .HasForeignKey(q => q.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QuoteItem>()
                .HasOne(qi => qi.Quote)
                .WithMany(q => q.Items)
                .HasForeignKey(qi => qi.QuoteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuoteItem>()
                .HasOne(qi => qi.Device)
                .WithMany()
                .HasForeignKey(qi => qi.DeviceId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ProjectSystemType>()
    .HasKey(pst => new { pst.ProjectId, pst.SystemTypeId });

            modelBuilder.Entity<ProjectSystemType>()
                .HasOne(pst => pst.Project)
                .WithMany(p => p.ProjectSystemTypes)
                .HasForeignKey(pst => pst.ProjectId);

            modelBuilder.Entity<ProjectSystemType>()
                .HasOne(pst => pst.SystemType)
                .WithMany(st => st.ProjectSystemTypes)
                .HasForeignKey(pst => pst.SystemTypeId);


            // Decimal precision
            modelBuilder.Entity<QuoteItem>(entity =>
            {
                entity.Property(q => q.UnitPrice).HasPrecision(18, 2);
                entity.Property(q => q.TotalPrice).HasPrecision(18, 2);
                entity.Property(q => q.ProfitRatio).HasPrecision(5, 2);
                entity.Property(q => q.PriceAfterProfit).HasPrecision(18, 2);
            });
            // Device prices
            modelBuilder.Entity<Device>()
                .Property(d => d.ActualPrice)
                .HasPrecision(18, 2);  // 18 digits total, 2 decimals

            modelBuilder.Entity<Device>()
                .Property(d => d.SalesPrice)
                .HasPrecision(18, 2);

            // Quote financials
            modelBuilder.Entity<Quote>()
                .Property(q => q.Discount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Quote>()
                .Property(q => q.InstallationFee)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Quote>()
                .Property(q => q.FinalTotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Quote>()
                .Property(q => q.NetProfit)
                .HasPrecision(18, 2);

            modelBuilder.Entity<SystemType>().HasData(
    new SystemType { Id = 1, Name = "KNX" },
    new SystemType { Id = 2, Name = "BusPro" },
    new SystemType { Id = 3, Name = "Wireless" },
    new SystemType { Id = 4, Name = "Low Current" },
    new SystemType { Id = 5, Name = "Smart Wi-Fi" }
);

            //modelBuilder.Entity<ProjectSystemType>().HasData(
            //    new ProjectSystemType { ProjectId = 1, SystemTypeId = 1 },
            //    new ProjectSystemType { ProjectId = 1, SystemTypeId = 5 }
            //);



            // Seed data
            modelBuilder.Entity<ProjectSystemType>().HasData(
    new ProjectSystemType { ProjectId = 1, SystemTypeId = 1 }, // KNX
    new ProjectSystemType { ProjectId = 1, SystemTypeId = 4 } // Smart Wi-Fi
);
            modelBuilder.Entity<Client>().HasData(
                new Client
                {
                    Id = 1,
                    Name = "Client A",
                    Mobile = "123456789",
                    Email = "a@email.com",
                    Address = "Amman",
                    CreatedByUserId = "22a43c82-7050-4724-bcb5-c4e323bff67f"
                }
            );

            modelBuilder.Entity<Project>().HasData(
                new Project { Id = 1, ClientId = 1, Name = "Project X"}
            );

            modelBuilder.Entity<Device>().HasData(
                new Device { Id = 1, Name = "Panel 1", Type = "Panel", SystemType = "KNX", StockQuantity = 10, ActualPrice = 50m },
                new Device { Id = 2, Name = "Sensor 1", Type = "Sensor", SystemType = "KNX", StockQuantity = 20, ActualPrice = 15m }
            );
        }
    }
}
