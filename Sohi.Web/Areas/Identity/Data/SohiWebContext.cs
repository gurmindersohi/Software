using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sohi.Models;
using Sohi.Web.Models;

namespace Sohi.Web.Data
{
    public class SohiWebContext : IdentityDbContext<User>
    {
        public SohiWebContext(DbContextOptions<SohiWebContext> options)
            : base(options)
        {
        }


        public DbSet<User> Users { get; set; }

        //public DbSet<Account.Account> Accounts { get; set; }

        //public DbSet<SocialMedia.SocialMedia> SocialMediaAccounts { get; set; }

        public DbSet<Lead> Leads { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<User>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Role");
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
