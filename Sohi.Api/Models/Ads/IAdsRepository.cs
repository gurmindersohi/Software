using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sohi.Models;

namespace Sohi.Api.Models.Ads
{
    public interface IAdsRepository
    {
        Task<IEnumerable<AdAccount>> GetAllAccounts(string accountid);

        Task<AdAccount> SaveAccount(AdAccount account);
    }
}
