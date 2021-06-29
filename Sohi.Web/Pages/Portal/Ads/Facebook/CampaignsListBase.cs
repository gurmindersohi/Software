using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Sohi.Models;
using Sohi.Web.Services.Ads;

namespace Sohi.Web.Pages.Portal.Ads.Facebook
{
    public class CampaignsListBase : ComponentBase
    {
        [CascadingParameter(Name = "AdsProfile")]
        public Profile Profile { get; set; }

        [Parameter]
        public string AccountId { get; set; }

        public List<Campaign> Campaigns { get; set; }

        [Inject]
        public IAdAccountService AdAccountService { get; set; }

        private string EndPoint { get; set; }

        [Inject]
        public IConfiguration _config { get; set; }


        public bool flag { get; set; } = false;


        protected override async Task OnParametersSetAsync()
        {
            EndPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

            await GetCampaigns();

        }

        private async Task GetCampaigns()
        {
            var result = await AdAccountService.GetAllCampaigns(Profile.Id, EndPoint, Profile.Token);

            if (result != null)
            {
                Campaigns = result;
            }
            else {
                flag = true;
            }
        }

    }
}
