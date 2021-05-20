using System;
using System.Threading.Tasks;
using Sohi.Models;

namespace Sohi.Api.Models.Settings
{
    public interface ISettingsRepository
    {
        Task<Account> GetAccount(Guid accountid);

        Task<Account> UpdateAccount(Account account);
    }
}
