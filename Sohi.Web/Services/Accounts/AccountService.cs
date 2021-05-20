using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Sohi.Models;

namespace Sohi.Web.Services.Accounts
{
    public class AccountService : IAccountService
    {
        private readonly HttpClient httpClient;

        public AccountService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<Account> GetAccount(Guid accountid)
        {
            var result = await httpClient.GetJsonAsync<Account>($"api/accounts/{accountid}");

            return result;
        }

        public async Task<Account> CreateAccount(Account newAccount)
        {
            return await httpClient.PostJsonAsync<Account>("api/accounts/", newAccount);
        }

        public async Task<Account> UpdateAccount(Account updatedAccount)
        {
            return await httpClient.PutJsonAsync<Account>("api/accounts/", updatedAccount);
        }

    }
}
