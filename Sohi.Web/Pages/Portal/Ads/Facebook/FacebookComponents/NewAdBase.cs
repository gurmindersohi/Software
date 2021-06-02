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
    public class NewAdBase : ComponentBase
    {
        [Parameter]
        public string AdsetId { get; set; }

        [Parameter]
        public string AccountId { get; set; }

        [Parameter]
        public string CampaignId { get; set; }

        public Ad Ad { get; set; } = new Ad();

        public AdImage SelectedImage { get; set; }

        protected AdImages OpenAdImagesModalConfirmation { get; set; }

        protected void AdImagesSelected()
        {
            OpenAdImagesModalConfirmation.Show();
        }

        protected void ImageSelected_Click(AdImage selectedImage)
        {
            if (selectedImage != null)
            {
                SelectedImage = selectedImage;
            }
        }

        protected async Task HandleValidSubmit()
        {

        }
    }
}
