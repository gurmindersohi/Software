using System;
using Microsoft.AspNetCore.Components;

namespace Sohi.Web.Pages.Portal.Ads.Facebook
{
    public class CampaignsListBase : ComponentBase
    {

        [Parameter]
        public string AdAccountId { get; set; }
    }
}
