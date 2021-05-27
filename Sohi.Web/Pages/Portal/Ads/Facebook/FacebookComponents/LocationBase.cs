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
    public class LocationBase : ComponentBase
    {

        [Parameter]
        public string location { get; set; }

        [CascadingParameter(Name = "AdsProfile")]
        public Profile Profile { get; set; }

        [Inject]
        public IConfiguration _config { get; set; }

        [Inject]
        public IAdAccountService AdAccountService { get; set; }

        public List<FacebookLocation> facebookLocation { get; set; }


        public FacebookLocation SelectedLocation { get; set; }


        protected async Task LocationSelected(FacebookLocation selectedLocation)
        {

            SelectedLocation = selectedLocation;

            facebookLocation.Clear();


        }

        protected async Task DeleteSelectedLocation(FacebookLocation selectedLocation)
        {

            SelectedLocation = null;
        }

        protected async Task HandleKeyUp(KeyboardEventArgs e)
        {
            string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

            var result = await AdAccountService.SearchLocation(Profile.Token, endPoint, location);

            if (result != null)
            {
                facebookLocation = result;

                StateHasChanged();
            }
        }

    }
}
