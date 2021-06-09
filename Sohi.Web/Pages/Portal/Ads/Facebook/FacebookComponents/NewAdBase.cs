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

        [CascadingParameter(Name = "AdsProfile")]
        public Profile Profile { get; set; }

        [Inject]
        public IConfiguration _config { get; set; }

        [Inject]
        public IAdAccountService AdAccountService { get; set; }


        public Ad Ad { get; set; } = new Ad();

        public AdImage SelectedImage { get; set; }

        protected AdImages OpenAdImagesModalConfirmation { get; set; }

        protected void AdImagesSelected()
        {
            OpenAdImagesModalConfirmation.Show();
        }

        protected void RemoveSelectedAdImage()
        {

            SelectedImage = null;

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
            string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

            object ObjectStorySpec = GetObjectStorySpec();

            var content = new
            {
                name = Ad.Name,
                object_story_spec = ObjectStorySpec,
                access_token = "EAABsbCS1iHgBALXFAjOXP0VXviKoOxDu173aD2GfDrYVC48kAZB46KJOA0Uxw1WgM7ZBCfn9GAQCYECaAxufZB7xz94w6K8DMZBgOH4xi30xwYzTwVsNUH4z5dViv6V9W9Gu3a6w6GJhFl1L79ML0sSK5895qHOP5cnvwaQdPvi5G0FZC0OjZC"
            };


            var AdCreativeId = await AdAccountService.CreateFacebookAdCreative(AccountId, endPoint, content);

            if (AdCreativeId != null)
            {

                var AdId = await CreateFacebookAd(endPoint, AdCreativeId);

            }

        }

        private object GetObjectStorySpec()
        {

            var data = new
            {
                image_hash = SelectedImage.Hash,
                link = Ad.WebsitrUrl,
                message = Ad.PrimaryText,
            };

            var ObjectData = new
            {
                link_data = data,
                page_id = "102420827994118"
            };


            return ObjectData;

        }

        protected async Task<string> CreateFacebookAd(string endPoint, string creativeId)
        {
            var data = new
            {
                creative_id = creativeId,
            };

            var content = new
            {
                name = Ad.Name,
                adset_id = AdsetId,
                creative = data,
                status = "PAUSED",
                access_token = "EAABsbCS1iHgBALXFAjOXP0VXviKoOxDu173aD2GfDrYVC48kAZB46KJOA0Uxw1WgM7ZBCfn9GAQCYECaAxufZB7xz94w6K8DMZBgOH4xi30xwYzTwVsNUH4z5dViv6V9W9Gu3a6w6GJhFl1L79ML0sSK5895qHOP5cnvwaQdPvi5G0FZC0OjZC"
            };

            var adId = await AdAccountService.CreateFacebookAd(AccountId, endPoint, content);

            return adId;
        }

    }
}
