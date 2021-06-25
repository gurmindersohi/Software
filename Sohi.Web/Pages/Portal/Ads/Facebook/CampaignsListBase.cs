using System;
using Microsoft.AspNetCore.Components;
using Sohi.Models;

namespace Sohi.Web.Pages.Portal.Ads.Facebook
{
    public class CampaignsListBase : ComponentBase
    {
        [CascadingParameter(Name = "AdsProfile")]
        public Profile Profile { get; set; }

        [Parameter]
        public string AccountId { get; set; }


    }
}
