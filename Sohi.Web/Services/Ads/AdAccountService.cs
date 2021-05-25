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

                    account.UserAccountId = item["userAccountId"].ToString();

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

        public async Task<List<Profile>> GetFacebookAdAccounts(string accesstoken, string endPoint)
        {
            List<Profile> accounts = new List<Profile>();

            string url = string.Format(endPoint + "/me/adaccounts?fields=id,name&access_token={0}&limit=100", accesstoken);

            //string facebook_EndPoint = string.Format(FacebookAPIEndpoints.GetFacebookAccounts + "?access_token={0}&fields=id,name,about&limit=100", access_token);

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var parsedobj = (JObject)JsonConvert.DeserializeObject(jsonResponse);

                foreach (var item in parsedobj["data"])
                {
                    Profile account = new Profile();

                    account.Id = item["id"].ToString();
                    account.Name = item["name"].ToString();

                    //if (item["picture"]["data"]["url"] != null)
                    //{
                    //    page.Image = item["picture"]["data"]["url"].ToString();
                    //}

                    account.Token = accesstoken;

                    accounts.Add(account);
                }

                return accounts;
            }
            else
            {
                return null;
            }
        }

        public async Task<Profile> GetFacebookAdAccount(string accesstoken, string endPoint)
        {
            Profile account = new Profile();

            string url = string.Format(endPoint + "?fields=id,name&access_token={0}", accesstoken);

            //string facebook_EndPoint = string.Format(FacebookAPIEndpoints.GetFacebookAccounts + "?access_token={0}&fields=id,name,about&limit=100", access_token);

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var parsedobj = (JObject)JsonConvert.DeserializeObject(jsonResponse);

                account.Id = parsedobj["id"].ToString();
                account.Name = parsedobj["name"].ToString();

                //if (item["picture"]["data"]["url"] != null)
                //{
                //    page.Image = item["picture"]["data"]["url"].ToString();
                //}

                account.Token = accesstoken;


                return account;
            }
            else
            {
                return null;
            }
        }



        public async Task<string> CreateFacebookCampaign(string AccountId, string endPoint, FormUrlEncodedContent content)
        {

            string url = string.Format(endPoint + "/{0}/campaigns", AccountId);

            var response = await httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = response.Content.ReadAsStringAsync().Result;
                var parsedobj = (JObject)JsonConvert.DeserializeObject(jsonResponse);

                return parsedobj["id"].ToString();
            }
            else
            {
                return null;
            }
        }

    }
}
