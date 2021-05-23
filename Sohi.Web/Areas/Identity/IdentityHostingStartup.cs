using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sohi.Web.Data;
using Sohi.Web.Models;
using Sohi.Web.Services.Accounts;
using Sohi.Web.Services.Emails;

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
                        context.Configuration.GetConnectionString("SohiDbConnection")));

                services.AddIdentity<User, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<SohiWebContext>().AddDefaultTokenProviders();

                services.AddTransient<IEmailSender, EmailSender>();
                //services.AddScoped<IAccountService, AccountService>();
            });
        }
    }
}