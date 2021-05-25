using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Sohi.Models;

namespace Sohi.Web.Services.Ads
{
    public interface IAdAccountService
    {
        Task<AdAccount> SaveAccount(AdAccount account);

        Task<List<AdAccount>> GetAllAccounts(string accountid);

        Task<List<Profile>> GetFacebookAdAccounts(string accesstoken, string endPoint);

        Task<Profile> GetFacebookAdAccount(string accesstoken, string endPoint);

        //Create Facebook Ads

        Task<string> CreateFacebookCampaign(string AccountId, string endPoint, FormUrlEncodedContent content);
    }
}
