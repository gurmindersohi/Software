using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Sohi.Models;

namespace Sohi.Web.Services.Settings
{
    public class SettingsService : ISettingsService
    {
        private readonly HttpClient httpClient;

        public SettingsService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<Account> GetAccount(Guid accountid)
        {
            var result = await httpClient.GetJsonAsync<Account>($"api/settings/{accountid}");

            return result;
        }

        public async Task<Account> UpdateAccount(Account updatedAccount)
        {
            return await httpClient.PutJsonAsync<Account>("api/settings/", updatedAccount);
        }

    }

    
}
