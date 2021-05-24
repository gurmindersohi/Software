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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }
    }
}
