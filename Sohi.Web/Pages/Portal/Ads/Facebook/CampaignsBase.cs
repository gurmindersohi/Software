using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Sohi.Models;

namespace Sohi.Web.Pages.Portal.Ads.Facebook
{
    public class CampaignsBase : ComponentBase
    {

        public Campaign Campaign { get; set; } = new Campaign();

        protected async Task HandleValidSubmit()
        {
            

        }
    }
}
