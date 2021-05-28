using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Sohi.Models;
using Sohi.Web.Models;
using Sohi.Web.Services.Ads;

namespace Sohi.Web.Pages.Portal.Ads.Facebook.FacebookComponents
{
    public class NewCampaignBase : ComponentBase
    {
        public Campaign Campaign { get; set; } = new Campaign();

        [CascadingParameter(Name = "AdsProfile")]
        public Profile Profile { get; set; }

        [Parameter]
        public string AccountId { get; set; }

        public string CampaignId { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IAdAccountService AdAccountService { get; set; }

        [Inject]
        public UserManager<User> userManager { get; set; }

        [Inject]
        public IConfiguration _config { get; set; }

        protected async Task HandleValidSubmit()
        {
            string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

            var content = new
            {
                name = Campaign.Name,
                objective = Campaign.Objective,
                status = "PAUSED",
                special_ad_categories = "NONE",
                access_token = Profile.Token

            };


            CampaignId = await AdAccountService.CreateFacebookCampaign(AccountId, endPoint, content);


            if (CampaignId != null)
            {
                NavigationManager.NavigateTo("/Portal/Ads/Facebook/" + AccountId + "/Create/Adsets/" + CampaignId);
            }

        }



        //protected async Task HandleValidSubmit()
        //{
        //    string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;
        //    var content = new FormUrlEncodedContent(new[]
        //                   {
        //                        new KeyValuePair<string, string>("name", Campaign.Name),
        //                        new KeyValuePair<string, string>("objective", Campaign.Objective),
        //                        new KeyValuePair<string, string>("status", "PAUSED"),
        //                        new KeyValuePair<string, string>("special_ad_categories", "NONE"),
        //                        new KeyValuePair<string, string>("access_token", Profile.Token)
        //                    });

        //    CampaignId = await AdAccountService.CreateFacebookCampaign(AccountId, endPoint, content);


        //    if (CampaignId != null)
        //    {
        //        NavigationManager.NavigateTo("/Portal/Ads/Facebook/" + AccountId + "/Create/Adsets/" + CampaignId);
        //    }

        //}
    }
}
