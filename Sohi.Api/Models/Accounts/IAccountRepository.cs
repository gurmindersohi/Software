using System;
using System.Threading.Tasks;
using Sohi.Models;

namespace Sohi.Api.Models.Accounts
{
    public interface IAccountRepository
    {
        Task<Account> GetAccount(Guid accountid);

        Task<Account> UpdateAccount(Account account);

        Task<Account> CreateAccount(Account newAccount);

        Task<Account> GetAccountByEmail(string email);
    }
}
