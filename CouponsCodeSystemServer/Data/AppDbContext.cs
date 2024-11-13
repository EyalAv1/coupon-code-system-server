using CouponsCodeSystemServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CouponsCodeSystemServer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply unique constraint on the Code column
            modelBuilder.Entity<Coupon>()
                .HasIndex(c => c.Code)
                .IsUnique();
        }
    }
}
