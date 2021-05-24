using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sohi.Models;
using System.Collections.Generic;


namespace Sohi.Web.Services.Ads
{
    public class AdAccountService : IAdAccountService
    {
        private readonly HttpClient httpClient;


        [Inject]
        public IConfiguration configuration { get; set; }


        public AdAccountService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<List<AdAccount>> GetAllAccounts(string accountid)
        {
            Guid id = new Guid(accountid);

            List<AdAccount> accounts = new List<AdAccount>();

            var response = await httpClient.GetAsync($"api/Ads/{accountid}");

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var parsedobj = (JArray)JsonConvert.DeserializeObject(jsonResponse);

                foreach (var item in parsedobj)
                {

                    AdAccount account = new AdAccount();
                    account.Id = new Guid(item["id"].ToString());

                    account.UserAccountId = item["useraccountid"].ToString();

                    account.Name = item["name"].ToString();

                    account.Image = item["image"].ToString();

                    account.Type = item["type"].ToString();
                    account.AccessToken = item["accessToken"].ToString();
                    account.Secret = item["secret"].ToString();
                    account.CreatedOn = DateTime.Parse(item["createdOn"].ToString());
                    account.TokenExpiryDate = DateTime.Parse(item["tokenExpiryDate"].ToString());
                    account.Email = item["email"].ToString();
                    account.UserId = item["userId"].ToString();
                    account.AccountId = item["accountId"].ToString();

                    accounts.Add(account);

                }
                return accounts;
            }
            else
            {
                return null;
            }

        }

        public async Task<AdAccount> SaveAccount(AdAccount account)
        {
            return await httpClient.PostJsonAsync<AdAccount>("api/Ads", account);
        }
    }
}
