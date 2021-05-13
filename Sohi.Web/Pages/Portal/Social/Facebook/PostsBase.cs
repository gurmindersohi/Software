using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Sohi.Models;
using Sohi.Web.Services.Social;

namespace Sohi.Web.Pages.Portal.Social.Facebook
{
    public class PostsBase : ComponentBase
    {

        [Inject]
        public ISocialService SocialService { get; set; }

        [Inject]
        public IConfiguration _config { get; set; }

        public List<Post> Posts { get; set; }

        private string EndPoint { get; set; }


        //[CascadingParameter(Name = "Id")]
        //public string Id { get; set; }


        //[CascadingParameter(Name = "PageToken")]
        //public string PageToken { get; set; }

        [CascadingParameter(Name = "SocialProfile")]
        public Profile Profile { get; set; }

        [Parameter]
        public string PageId { get; set; }
        //protected async override Task OnInitializedAsync()
        //{

        //    EndPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

        //    var result = await PostsAsync();

        //    if (result != null)
        //    {
        //        Posts = result;
        //    }

        //}

        protected override void OnParametersSet()
        {
            Posts = null;
        }


        protected async override Task OnParametersSetAsync()
        {

            EndPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

            var result = await PostsAsync();

            if (result != null)
            {
                Posts = result;
            }
        }


        private async Task<List<Post>> PostsAsync()
        {
            try
            {
                List<Post> posts = new List<Post>();

                string pageid = Profile.Id;
                //string accessToken = AccessToken;

                //var pagetoken = await SocialService.GenerateFacebookPageTokenAsync(pageid, accessToken, EndPoint);

                string PageToken = Profile.Token;

                if (PageToken != null)
                {
                    posts = await SocialService.GetFacebookPosts(pageid, PageToken, EndPoint);
                }

                return posts;

            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
