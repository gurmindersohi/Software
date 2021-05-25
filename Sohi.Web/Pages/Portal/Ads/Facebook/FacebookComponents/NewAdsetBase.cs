using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Sohi.Models;

namespace Sohi.Web.Pages.Portal.Ads.Facebook.FacebookComponents
{
    public class NewAdsetBase : ComponentBase
    {

        [Parameter]
        public string AccountId { get; set; }

        [Parameter]
        public string CampaignId { get; set; }


    }
}
