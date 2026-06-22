#nullable enable
using Microsoft.EntityFrameworkCore;
using noithat_ducanh.Models;

namespace noithat_ducanh.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Post> Posts { get; set; } = null!;
        public DbSet<ContactRequest> ContactRequests { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<SystemSetting> SystemSettings { get; set; } = null!;
        public DbSet<SocialLink> SocialLinks { get; set; } = null!;
        public DbSet<ProductComparison> ProductComparisons { get; set; } = null!;
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<Admin> Admins { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Optional: configure decimal precision for price properties
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasConversion<double?>(); // SQLite doesn't natively support decimal as decimal but conversions to double/real work fine.
                
            modelBuilder.Entity<Product>()
                .Property(p => p.OldPrice)
                .HasConversion<double?>();
        }
    }
}
