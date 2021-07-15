using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sohi.Models;


namespace Sohi.Api.Models.Billing
{
    public class BillingRepository : IBillingRepository
    {
        private readonly AppDbContext context;

        public BillingRepository(AppDbContext context)
        {
            this.context = context;
        }
        public async Task<Account> GetAccount(Guid accountid)
        {
            return await context.Accounts.FirstOrDefaultAsync(a => a.AccountId == accountid);
        }

        public async Task<Account> UpdateAccount(Account account)
        {
            var result = await context.Accounts.FirstOrDefaultAsync(a => a.AccountId == account.AccountId);

            if (result != null)
            {
                result.CustomerId = account.CustomerId;
                result.SubscriptionId = account.SubscriptionId;

                await context.SaveChangesAsync();
                return result;

            }
            return null;
        }

    }
}
