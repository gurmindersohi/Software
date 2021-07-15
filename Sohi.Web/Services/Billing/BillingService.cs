using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sohi.Models;
using System.Collections.Generic;
using System.Net.Http.Json;

namespace Sohi.Web.Services.Billing
{
    public class BillingService : IBillingService
    {
        private readonly HttpClient httpClient;

        public BillingService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }


        public async Task<Account> UpdateBillingDetails(Account updatedAccount)
        {
            return await httpClient.PutJsonAsync<Account>("api/billing/", updatedAccount);
        }

    }
}
