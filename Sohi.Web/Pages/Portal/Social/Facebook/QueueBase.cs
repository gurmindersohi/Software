using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Sohi.Models;
using Sohi.Web.Models;
using Sohi.Web.Services.Social;

namespace Sohi.Web.Pages.Portal.Social.Facebook
{
    public class QueueBase : ComponentBase
    {

        private string EndPoint { get; set; }

        [Inject]
        public IConfiguration _config { get; set; }

        [Parameter]
        public string PageId { get; set; }

        [Inject]
        public ISocialService SocialService { get; set; }

        public List<Post> Posts { get; set; }


        public bool flag { get; set; } = false;


        [CascadingParameter(Name = "SocialProfile")]
        public Profile Profile { get; set; }

        [CascadingParameter(Name = "CurrentUser")]
        public User user { get; set; }

        public AdImage SelectedImage { get; set; }

        protected SocialImages OpenSocialImagesModalConfirmation { get; set; }

        protected override void OnParametersSet()
        {
            Posts = null;
        }

        protected async override Task OnParametersSetAsync()
        {

            EndPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

            string pageid = Profile.Id;

            string PageToken = Profile.Token;


            if (PageToken != null)
            {

                //Profile = await SocialService.GetFacebookPage(pageid, PageToken, EndPoint);

                var result = await SocialService.GetFacebookScheduledPosts(pageid, PageToken, EndPoint);

                if (result != null)
                {
                    Posts = result;
                }
                else
                {
                    flag = true;
                }
            }
           
        }


        protected void ImageSelected()
        {
            OpenSocialImagesModalConfirmation.Show();
        }

        protected void RemoveSelectedImage()
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


        //private async Task<List<Post>> GetScheduledPosts(string pageid, string pagetoken, string EndPoint)
        //{
        //    try
        //    {
        //        List<Post> posts = new List<Post>();


        //        posts = await 


        //        return posts;

        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}
    }
}
