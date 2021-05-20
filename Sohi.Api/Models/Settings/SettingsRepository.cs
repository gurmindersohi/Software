using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sohi.Models;

namespace Sohi.Api.Models.Settings
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly AppDbContext context;

        public SettingsRepository(AppDbContext context)
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
                result.AccountName = account.AccountName;
                result.Phone = account.Phone;
                result.Address = account.Address;
                result.City = account.City;
                result.Province = account.Province;
                result.Country = account.Country;
                result.PostalCode = account.PostalCode;
                result.Logo = account.Logo;
                result.ModifiedBy = account.ModifiedBy;
                result.ModifiedOn = account.ModifiedOn;

                await context.SaveChangesAsync();
                return result;

            }
            return null;
        }


    }
}
