using System;
using System.Threading.Tasks;
using Sohi.Models;

namespace Sohi.Api.Models.Billing
{
    public interface IBillingRepository
    {
        Task<Account> GetAccount(Guid accountid);

        Task<Account> UpdateAccount(Account account);
    }
}
