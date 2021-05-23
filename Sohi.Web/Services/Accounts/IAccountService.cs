using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sohi.Models;
using Sohi.Web.Models;

namespace Sohi.Web.Services.Accounts
{
    public interface IAccountService
    {
        Task<Account> GetAccount(Guid accountid);

        Task<Account> CreateAccount(Account newAccount);

        Task<Account> UpdateAccount(Account updatedAccount);


    }
}
