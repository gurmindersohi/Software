using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Sohi.Models;
using Sohi.Web.Services.Ads;

namespace Sohi.Web.Pages.Portal.Ads.Facebook.FacebookComponents
{
    public class AdImagesBase : ComponentBase
    {

        protected bool ShowAdImagesModal { get; set; }

        public Adset Adset { get; set; } = new Adset();

        //public string AccountId { get; set; } = "act_2572420379713537";


        public string Active { get; set; }

        [CascadingParameter(Name = "AdsProfile")]
        public Profile Profile { get; set; }

        [Parameter]
        public string AccountId { get; set; }

        [Inject]
        public IAdAccountService AdAccountService { get; set; }

        [Inject]
        public IConfiguration _config { get; set; }

        public List<AdImage> AdImages { get; set; }

        private string EndPoint { get; set; }

        public AdImage SelectedImage { get; set; }

        [Parameter]
        public EventCallback<AdImage> SelectionChanged { get; set; }

        public void Show()
        {
            ShowAdImagesModal = true;
            StateHasChanged();
        }

        protected void OnSelectionCancel(bool value)
        {
            ShowAdImagesModal = false;
        }

        protected async Task OnSelectionChange()
        {
            //ShowAdImagesModal = false;
            await SelectionChanged.InvokeAsync(SelectedImage);
        }

        protected void SelectImage(AdImage image)
        {
            if (image != null) {
                SelectedImage = image;
            }
        }

        protected async override Task OnParametersSetAsync()
        {

            EndPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

            var result = await GetAdImages();

            if (result != null)
            {
                AdImages = result;
            }
        }

        private async Task<List<AdImage>> GetAdImages()
        {
            try
            {
                List<AdImage> adImages = new List<AdImage>();

                //string UserToken = "EAAQGsEzdprgBAODfCkwKRr9G020E7i6NpZBW7IW9uZB50JMfQYFk6rB2Hupm7jEIzmyofDvoGxTeW0gWILGToAI2M9vdZBOeZB5e8kuoQCZCMkByR6iCzkKhuwWppDD6M9K31b6ZBPMfiheF81P3JI1mrrYRjZCbZBm0xKp5UM0UaUWBsG0GtFhOIG5D5vlZBLWrLexzGIzEZBHAZDZD";

                if (Profile.Token != null)
                {
                    adImages = await AdAccountService.GetFacebookAdImages(AccountId, Profile.Token, EndPoint);
                }

                return adImages;

            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
