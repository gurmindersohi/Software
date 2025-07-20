using System;
using Microsoft.EntityFrameworkCore;
using Sohi.Models;

namespace Sohi.Api.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Lead> Leads { get; set; }
        public DbSet<SocialMedia> SocialMediaAccounts { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<AdAccount> AdAccounts { get; set; }

        public DbSet<Plan> Plans { get; set; }
        public DbSet<Post> Posts { get; set; } // Add this if Post is not already included

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Plan>(entity =>
            {
                entity.Property(p => p.Price).HasPrecision(18, 2); // Example: 18 digits, 2 decimal places
                entity.Property(p => p.Tax).HasPrecision(18, 2);
                entity.Property(p => p.Total).HasPrecision(18, 2);
            });
        }
    }
}
