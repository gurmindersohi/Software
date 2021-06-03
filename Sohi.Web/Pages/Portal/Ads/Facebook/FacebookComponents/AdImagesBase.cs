using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sohi.Models;
using Sohi.Web.Services.Ads;

namespace Sohi.Web.Pages.Portal.Ads.Facebook.FacebookComponents
{
    public class AdImagesBase : ComponentBase
    {

        protected bool ShowAdImagesModal { get; set; }

        public Adset Adset { get; set; } = new Adset();

        //public string AccountId { get; set; } = "act_2572420379713537";


        public string Active { get; set; } = "";

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

        //public AdImage SelectedImage { get; set; }

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

        protected async Task OnSelectionChange(AdImage image)
        {
            ShowAdImagesModal = false;

            if (image != null)
            {
                await SelectionChanged.InvokeAsync(image);
            }

        }

        private List<IBrowserFile> loadedFiles = new();
        private long maxFileSize = 5120000;
        private int maxAllowedFiles = 3;
        protected bool isLoading = false;


        protected async Task LoadFiles(InputFileChangeEventArgs e)
        {
            isLoading = true;
            loadedFiles.Clear();

            List<string> imgUrls = new List<string>();
            List<FileData> fileData = new List<FileData>();

            foreach (var file in e.GetMultipleFiles(maxAllowedFiles))
            {
                try
                {
                    loadedFiles.Add(file);

                    var buffers = new byte[file.Size];
                    await file.OpenReadStream(maxFileSize).ReadAsync(buffers);
                    string imageType = file.ContentType;
                    //string imgUrl = $"data:{imageType};base64,{Convert.ToBase64String(buffers)}";

                    string imgUrl = Convert.ToBase64String(buffers);

                    var content = new
                    {
                        name = file.Name,
                        bytes = imgUrl,
                        access_token = Profile.Token
                    };


                    var result = await AdAccountService.UploadImageToFacebookAdAccount(EndPoint, AccountId, content);

                    var images = await GetAdImages();

                    if (images != null)
                    {
                        AdImages = images;
                    }

                    StateHasChanged();
                }
                catch (Exception ex)
                {
                   
                }
            }

            isLoading = false;
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
