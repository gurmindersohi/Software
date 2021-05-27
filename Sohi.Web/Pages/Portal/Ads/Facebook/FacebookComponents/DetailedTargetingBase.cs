using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Configuration;
using Sohi.Models;
using Sohi.Web.Services.Ads;


namespace Sohi.Web.Pages.Portal.Ads.Facebook.FacebookComponents
{
    public class DetailedTargetingBase : ComponentBase
    {
        [Parameter]
        public string location { get; set; }


        public string SearchText { get; set; }

        [CascadingParameter(Name = "AdsProfile")]
        public Profile Profile { get; set; }


        [Parameter]
        public string AccountId { get; set; }


        [Inject]
        public IConfiguration _config { get; set; }

        [Inject]
        public IAdAccountService AdAccountService { get; set; }

        public List<Targeting> DetailedTargetings { get; set; }


        public Targeting DetailedTargeting { get; set; }


        public List<Targeting> SelectedDetailedTargeting { get; set; }


        protected async Task TargetSelected(Targeting targeting)
        {

            DetailedTargetings.Clear();

            SearchText = String.Empty;

            var item = (SelectedDetailedTargeting.Find(t => t.Id == targeting.Id));

            if (item == null)
            {
                SelectedDetailedTargeting.Add(item);
            }
            

            SelectedDetailedTargeting.Add(targeting);




        }

        protected async Task DeleteSelectedTarget(Targeting targeting)
        {
            var item = (SelectedDetailedTargeting.Find(t => t.Id == targeting.Id));

            if (item == null)
            {
                SelectedDetailedTargeting.Remove(item);
            }

        }

        protected async Task HandleKeyUp(KeyboardEventArgs e)
        {
            string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value + "/" + AccountId;

            var result = await AdAccountService.SearchDetailedTargeting(Profile.Token, endPoint, SearchText);

            if (result != null)
            {
                DetailedTargetings = result;

                StateHasChanged();
            }
        }
    }
}
