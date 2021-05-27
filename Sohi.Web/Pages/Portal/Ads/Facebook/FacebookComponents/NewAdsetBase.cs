using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Sohi.Models;
using Sohi.Web.Services.Ads;

namespace Sohi.Web.Pages.Portal.Ads.Facebook.FacebookComponents
{
    public class NewAdsetBase : ComponentBase
    {

        [Parameter]
        public string AccountId { get; set; }

        [Parameter]
        public string CampaignId { get; set; }

        public Adset Adset { get; set; } = new Adset();

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public bool ScheduleEndDate { get; set; } = false;

        [CascadingParameter(Name = "AdsProfile")]
        public Profile Profile { get; set; }

        [Inject]
        public IConfiguration _config { get; set; }

        [Inject]
        public IAdAccountService AdAccountService { get; set; }

        public List<FacebookLocation> facebookLocation { get; set; }

        //protected async Task SearchLocation()
        //{

        //    string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

        //    var result = await AdAccountService.SearchLocation(AccountId, endPoint, Pr);

        //    if (result != null)
        //    {
        //        facebookLocation = result;
        //    }


        //}



        protected async Task HandleValidSubmit()
        {
            //string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;
            //var content = new FormUrlEncodedContent(new[]
            //               {
            //                    new KeyValuePair<string, string>("name", Campaign.Name),
            //                    new KeyValuePair<string, string>("objective", Campaign.Objective),
            //                    new KeyValuePair<string, string>("status", "PAUSED"),
            //                    new KeyValuePair<string, string>("special_ad_categories", "NONE"),
            //                    new KeyValuePair<string, string>("access_token", Profile.Token)
            //                });

            //CampaignId = await AdAccountService.CreateFacebookCampaign(AccountId, endPoint, content);


            //if (CampaignId != null)
            //{
            //    NavigationManager.NavigateTo("/Portal/Ads/Facebook/" + AccountId + "/Create/Adsets/" + CampaignId);
            //}

        }

    }
}
