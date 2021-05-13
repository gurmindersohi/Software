using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Sohi.Models;
using Sohi.Web.Services.Social;

namespace Sohi.Web.Pages.Portal.Social.Facebook
{
    public class QueueBase : ComponentBase
    {

        private string EndPoint { get; set; }

        [Inject]
        public IConfiguration _config { get; set; }

        //[CascadingParameter(Name = "Id")]

        [Parameter]
        public string PageId { get; set; }

        [CascadingParameter(Name = "Token")]
        public string AccessToken { get; set; }

        [Inject]
        public ISocialService SocialService { get; set; }

        public List<Post> Posts { get; set; }

        public Profile Profile { get; set; }

        public bool flag { get; set; } = true;


        [CascadingParameter(Name = "Id")]
        public string Id { get; set; }


        [CascadingParameter(Name = "PageToken")]
        public string PageToken { get; set; }

        protected override void OnParametersSet()
        {
            Posts = null;
        }

        protected async override Task OnParametersSetAsync()
        {

            EndPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

            string pageid = Id;
           
            if (PageToken != null)
            {

                Profile = await SocialService.GetFacebookPage(pageid, PageToken, EndPoint);

                var result = await GetScheduledPosts(pageid, PageToken, EndPoint);

                if (result != null)
                {
                    Posts = result;
                }
                else
                {
                    flag = false;
                }
            }
           
        }

        private async Task<List<Post>> GetScheduledPosts(string pageid, string pagetoken, string EndPoint)
        {
            try
            {
                List<Post> posts = new List<Post>();

                
                posts = await SocialService.GetFacebookScheduledPosts(pageid, pagetoken, EndPoint);
                

                return posts;

            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
