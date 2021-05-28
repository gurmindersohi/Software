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

        public string SearchText { get; set; }

        [Parameter]
        public EventCallback<FacebookLocation> OnLocationSelection { get; set; }

        [CascadingParameter(Name = "AdsProfile")]
        public Profile Profile { get; set; }

        [Inject]
        public IConfiguration _config { get; set; }

        [Inject]
        public IAdAccountService AdAccountService { get; set; }

        public List<FacebookLocation> facebookLocation { get; set; }


        public List<FacebookLocation> SelectedLocation { get; set; } = new List<FacebookLocation>();


        protected async Task LocationSelected(FacebookLocation selectedLocation)
        {
            if (selectedLocation != null)
            {
                SelectedLocation.Add(selectedLocation);

                SearchText = String.Empty;

                facebookLocation.Clear();

                await OnLocationSelection.InvokeAsync(selectedLocation);
            }



        }

        protected async Task DeleteSelectedLocation(FacebookLocation selectedLocation)
        {
            var item = (SelectedLocation.Find(l => l.Key == selectedLocation.Key));

            if (item == null)
            {
                SelectedLocation.Remove(item);
            }
        }

        protected async Task HandleKeyUp(KeyboardEventArgs e)
        {
            string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

            var result = await AdAccountService.SearchLocation(Profile.Token, endPoint, SearchText);

            if (result != null)
            {
                facebookLocation = result;

                StateHasChanged();
            }
        }

    }
}
