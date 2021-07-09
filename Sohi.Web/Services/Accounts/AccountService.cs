using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Sohi.Models;
using Sohi.Web.Models;
using Sohi.Web.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Sohi.Web.Services.Accounts
{
    public class AccountService : IAccountService
    {
        private readonly SohiWebContext context;

        private List<Plans> _plansList;

        public AccountService()
        {
            _plansList = new List<Plans>()
            {
                new Plans() { Id = "25", Name = "basic", Type = "Basic",
                    BillingPeriod = "Monthly", Price="24" },
                new Plans() { Id = "12", Name = "premium", Type = "Premium",
                    BillingPeriod = "Monthly", Price="99" },
                new Plans() { Id = "1998", Name = "unlimited", Type = "Unlimited",
                    BillingPeriod = "Monthly", Price="299" },

            };

        }

        public AccountService(SohiWebContext context)
        {
            this.context = context;

            _plansList = new List<Plans>()
            {
                new Plans() { Id = "25", Name = "basic", Type = "Basic",
                    BillingPeriod = "Monthly", Price="24" },
                new Plans() { Id = "12", Name = "premium", Type = "Premium",
                    BillingPeriod = "Monthly", Price="99" },
                new Plans() { Id = "1998", Name = "unlimited", Type = "Unlimited",
                    BillingPeriod = "Monthly", Price="299" },

            };

        }

        private readonly HttpClient httpClient;

        public AccountService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<Account> GetAccount(Guid accountid)
        {
            var result = await httpClient.GetJsonAsync<Account>($"api/account/{accountid}");

            return result;
        }

        public async Task<Account> CreateAccount(Account newAccount)
        {
            return await httpClient.PostJsonAsync<Account>("api/account/", newAccount);
        }

        public async Task<Account> UpdateAccount(Account updatedAccount)
        {
            return await httpClient.PutJsonAsync<Account>("api/account/", updatedAccount);
        }

        public Plans GetPlans(string name)
        {
            var result = _plansList.FirstOrDefault(e => e.Name == name);

            return result;
        }

    }
}
