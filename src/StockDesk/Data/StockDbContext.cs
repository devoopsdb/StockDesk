using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using StockDesk.Common;
using StockDesk.Data.Entities;

namespace StockDesk.Data;

public class StockDbContext : DbContext
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Recipient> Recipients => Set<Recipient>();
    public DbSet<InventoryOperation> InventoryOperations => Set<InventoryOperation>();

    public StockDbContext()
    {
    }

    public StockDbContext(DbContextOptions<StockDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            AppPaths.EnsureDirectoriesCreated();
            optionsBuilder.UseSqlite($"Data Source={AppPaths.DatabasePath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(150);
            entity.HasIndex(c => c.Name).IsUnique();
        });

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(250);
            entity.Property(p => p.CurrentBalance).IsRequired();
            
            entity.ToTable(t => t.HasCheckConstraint("CK_Product_CurrentBalance", "\"CurrentBalance\" >= 0"));

            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Recipient configuration
        modelBuilder.Entity<Recipient>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(r => r.Name).IsUnique();
        });

        // InventoryOperation configuration
        modelBuilder.Entity<InventoryOperation>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.ProductNameSnapshot).IsRequired().HasMaxLength(250);
            entity.Property(o => o.CategoryNameSnapshot).IsRequired().HasMaxLength(150);
            entity.Property(o => o.RecipientNameSnapshot).HasMaxLength(200);
            entity.Property(o => o.Note).HasMaxLength(500);

            entity.HasOne(o => o.Product)
                  .WithMany(p => p.Operations)
                  .HasForeignKey(o => o.ProductId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(o => o.Recipient)
                  .WithMany(r => r.Operations)
                  .HasForeignKey(o => o.RecipientId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(o => o.Timestamp);
        });
    }

    public void InitializeDatabase()
    {
        AppPaths.EnsureDirectoriesCreated();
        Database.EnsureCreated();
        
        // Enable WAL mode & foreign keys for performance and safety
        Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
        Database.ExecuteSqlRaw("PRAGMA foreign_keys=ON;");
    }
}
