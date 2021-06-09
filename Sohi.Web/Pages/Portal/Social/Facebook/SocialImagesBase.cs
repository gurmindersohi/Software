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
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sohi.Models;
using Sohi.Web.Models;
using Sohi.Web.Services.Ads;


namespace Sohi.Web.Pages.Portal.Social.Facebook
{
    public class SocialImagesBase : ComponentBase
    {

        protected bool ShowSocialImagesModal { get; set; }

        public string Active { get; set; } = "";

        [Inject]
        public IConfiguration _config { get; set; }

        private string EndPoint { get; set; }

        [Parameter]
        public EventCallback<AdImage> SelectionChanged { get; set; }

        public List<AdImage> Images { get; set; }


        [Parameter]
        public User user { get; set; }

        CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=sohi;AccountKey=eNmdG+iAxpOeJR6tL7p6SbEmpX+aflNJab/Vmn/JSLl2WXdzq1svsBI5Wt1ZqXaznWibXltVHg/rVEPu/Omdew==;EndpointSuffix=core.windows.net");


        public void Show()
        {
            ShowSocialImagesModal = true;
            StateHasChanged();
        }

        protected void OnSelectionCancel(bool value)
        {
            ShowSocialImagesModal = false;
        }

        protected async Task OnSelectionChange(AdImage image)
        {
            ShowSocialImagesModal = false;

            if (image != null)
            {
                await SelectionChanged.InvokeAsync(image);
            }

        }

        protected async override Task OnParametersSetAsync()
        {

            EndPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

            var result = await GetImages();

            if (result != null)
            {
                Images = result;
            }

        }

        private async Task<List<AdImage>> GetImages()
        {
            try
            {
                List<AdImage> images = new List<AdImage>();

                var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

                var cloudBlobContainer = cloudBlobClient.GetContainerReference("clients");

                var cloudBlobDirectory = cloudBlobContainer.GetDirectoryReference(user.AccountId);


                var blobs = cloudBlobDirectory.ListBlobs(useFlatBlobListing: true);

                foreach (var blob in blobs)
                {
                    AdImage image = new AdImage();
                    image.Name = blob.Uri.Segments.Last();
                    image.Url = blob.Uri.AbsoluteUri;

                    images.Add(image);
                }

                return images;

            }
            catch (Exception ex)
            {
                return null;
            }
        }



        private List<IBrowserFile> loadedFiles = new();
        private long maxFileSize = 512000000000;
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

                    await UploadToAzureAsync(file);

                    var result = await GetImages();

                    if (result != null)
                    {
                        Images = result;
                    }

                }
                catch (Exception ex)
                {

                }
            }


            isLoading = false;
        }



        private async Task UploadToAzureAsync(IBrowserFile file)
        {
            var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

            var cloudBlobContainer = cloudBlobClient.GetContainerReference("clients");

            var cloudBlobDirectory = cloudBlobContainer.GetDirectoryReference(user.AccountId);

            var cloudBlockBlob = cloudBlobDirectory.GetBlockBlobReference(file.Name);
            cloudBlockBlob.Properties.ContentType = file.ContentType;

            await cloudBlockBlob.UploadFromStreamAsync(file.OpenReadStream(maxFileSize));
        }
    }
}
