using System;
using System.Collections.Generic;
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

        protected async override Task OnInitializedAsync()
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

                string pageid = "102420827994118";
                string accessToken = "EAAQGsEzdprgBAJKrwp34ZC9W48zJ2g0PhVnZA0MzEwGBTNKvyZAZAtuFwEkhzgmCkg6V2VIRpnQ9NXIrnZAo6ZBqpj3H2yni3D70ggZCbft5zUWoZCQEv6oRHWDMhTo7W0E5e91eKqmA6KTbSVneH66Y7ebP3ZA8GqdA11cVfU8Hh8AXKU7t7keRzfIQnnK8PHpbdgdEDCESQCgZDZD";

                var pagetoken = await SocialService.GenerateFacebookPageTokenAsync(pageid, accessToken, EndPoint);

                if (pagetoken != null)
                {
                    posts = await SocialService.GetFacebookPosts(pageid, accessToken, EndPoint);
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
