using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sohi.Models;

namespace Sohi.Api.Models.Ads
{
    public class AdsRepository : IAdsRepository
    {
        private readonly AppDbContext context;

        public AdsRepository(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<AdAccount>> GetAllAccounts(string accountid)
        {

            List<AdAccount> accounts = new List<AdAccount>();

            accounts = await context.AdAccounts.Where(a => a.AccountId == accountid).ToListAsync();

            return accounts;

        }

        public async Task<AdAccount> SaveAccount(AdAccount account)
        {
            context.AdAccounts.Add(account);
            await context.SaveChangesAsync();

            return account;
        }

    }
}
