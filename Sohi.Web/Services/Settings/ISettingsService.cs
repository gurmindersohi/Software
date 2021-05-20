using System;
using System.Threading.Tasks;
using Sohi.Models;

namespace Sohi.Web.Services.Settings
{
    public interface ISettingsService
    {
        Task<Account> GetAccount(Guid accountid);
        Task<Account> UpdateAccount(Account updatedAccount);
    }
}
