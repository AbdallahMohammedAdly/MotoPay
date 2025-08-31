using AutoLease.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutoLease.Persistence.Data;

public class AutoLeaseDbContext : IdentityDbContext<User>
{
    public AutoLeaseDbContext(DbContextOptions<AutoLeaseDbContext> options) : base(options)
    {
    }

    public DbSet<Car> Cars { get; set; }
    public DbSet<SalesAgent> SalesAgents { get; set; }
    public DbSet<Offer> Offers { get; set; }
    public DbSet<OfferApplication> OfferApplications { get; set; }
    public DbSet<CarInterest> CarInterests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Role)
                .IsRequired()
                .HasConversion<int>();

            entity.HasIndex(e => e.Email)
                .IsUnique();
        });

        // Configure Car entity
        modelBuilder.Entity<Car>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Make)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Model)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Color)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(e => e.VinNumber)
                .IsRequired()
                .HasMaxLength(17);

            entity.Property(e => e.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(1000);

            entity.HasIndex(e => e.VinNumber)
                .IsUnique();

            // Configure relationships
            entity.HasOne(e => e.Owner)
                .WithMany(u => u.CarsOwned)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.SalesAgent)
                .WithMany(s => s.AssignedCars)
                .HasForeignKey(e => e.SalesAgentId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Configure CarInterest entity
        modelBuilder.Entity<CarInterest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PreferredCallTime).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.DocumentPaths).HasMaxLength(4000);
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.Car)
                .WithMany()
                .HasForeignKey(e => e.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.CarId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configure SalesAgent entity
        modelBuilder.Entity<SalesAgent>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.PhoneNumber)
                .IsRequired()
                .HasMaxLength(15);

            entity.Property(e => e.Department)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.CommissionRate)
                .IsRequired()
                .HasColumnType("decimal(5,2)");

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.HasIndex(e => e.UserId)
                .IsUnique();

            // Configure relationship with User
            entity.HasOne(e => e.User)
                .WithOne(u => u.SalesAgent)
                .HasForeignKey<SalesAgent>(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // Configure Offer entity
        modelBuilder.Entity<Offer>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(1000);

            entity.Property(e => e.OriginalPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.DiscountedPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.DiscountPercentage)
                .IsRequired()
                .HasColumnType("decimal(5,2)");

            entity.Property(e => e.Terms)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(e => e.StartDate)
                .IsRequired();

            entity.Property(e => e.EndDate)
                .IsRequired();

            entity.Property(e => e.MaxApplications)
                .IsRequired();

            entity.Property(e => e.CurrentApplications)
                .IsRequired();

            entity.Property(e => e.IsActive)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt);

            // Configure relationships
            entity.HasOne(e => e.Car)
                .WithMany()
                .HasForeignKey(e => e.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SalesAgent)
                .WithMany()
                .HasForeignKey(e => e.SalesAgentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.CarId);
            entity.HasIndex(e => e.SalesAgentId);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.StartDate);
            entity.HasIndex(e => e.EndDate);
        });

        // Configure OfferApplication entity
        modelBuilder.Entity<OfferApplication>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ApplicationDate)
                .IsRequired();

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.Notes)
                .HasMaxLength(500);

            entity.Property(e => e.ReviewNotes)
                .HasMaxLength(1000);

            entity.Property(e => e.ReviewedAt);

            // Configure relationships
            entity.HasOne(e => e.Offer)
                .WithMany(o => o.Applications)
                .HasForeignKey(e => e.OfferId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ReviewedByUser)
                .WithMany()
                .HasForeignKey(e => e.ReviewedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(e => e.OfferId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.ApplicationDate);

            // Unique constraint to prevent duplicate applications
            entity.HasIndex(e => new { e.OfferId, e.UserId })
                .IsUnique();
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed roles
        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>().HasData(
            new Microsoft.AspNetCore.Identity.IdentityRole
            {
                Id = "1",
                Name = "Client",
                NormalizedName = "CLIENT"
            },
            new Microsoft.AspNetCore.Identity.IdentityRole
            {
                Id = "2", 
                Name = "SalesAgent",
                NormalizedName = "SALESAGENT"
            }
        );
    }
}