using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sohi.Web.Data;

[assembly: HostingStartup(typeof(Sohi.Web.Areas.Identity.IdentityHostingStartup))]
namespace Sohi.Web.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<SohiWebContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("SohiWebContextConnection")));

                services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<SohiWebContext>();
            });
        }
    }
}