using System;
using System.Threading.Tasks;
using Sohi.Models;

namespace Sohi.Web.Services.Accounts
{
    public interface IAccountService
    {
        Task<Account> GetAccount(Guid accountid);

        Task<Account> CreateAccount(Account newAccount);

        Task<Account> UpdateAccount(Account updatedAccount);

    }
}
