using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sohi.Models;

namespace Sohi.Api.Models.Social
{
    public class SocialRepository : ISocialRepository
    {
        private readonly AppDbContext context;

        public SocialRepository(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<SocialMedia> SaveToken(SocialMedia account)
        {
            context.SocialMediaAccounts.Add(account);
            await context.SaveChangesAsync();

            return account;
        }

        public async Task<SocialMedia> GetTokenByPlatformAsync(string accountid, string platform)
        { 

            SocialMedia accounts = new SocialMedia();

            accounts = await context.SocialMediaAccounts.FirstOrDefaultAsync(a => a.AccountId == accountid && a.Type == platform);

            return accounts;

        }
    }
}
