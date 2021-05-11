using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Sohi.Models;
using Sohi.Web.Models;
using Sohi.Web.Services.Social;

namespace Sohi.Web.Pages.Portal.Social
{
    public class CreateBase : ComponentBase
    {

        public Post Post { get; set; } = new Post();

        [Inject]
        public ISocialService SocialService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IConfiguration _config { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        [Inject]
        public UserManager<User> userManager { get; set; }

        public User user { get; set; }

        public string FacebookAccessToken { get; set; }

        public List<Profile> Profiles { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var authState = await authenticationStateTask;

            if (authState.User.Identity.IsAuthenticated)
            {
                user = await userManager.GetUserAsync(authState.User);
            }

            List<SocialMedia> accounts = await SocialService.GetAllTokens(user.AccountId.ToString());

            if (accounts != null)
            {
                //FacebookProfile = await SocialService.GetFacebookAccountAsync(account.AccessToken, endPoint);
                foreach (var account in accounts)
                {
                    if (account.Type == "FACEBOOK")
                    {
                        FacebookAccessToken = account.AccessToken;

                        string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

                        Profiles = await SocialService.GetFacebookPages(account.AccessToken, endPoint);

                        //FacebookProfile = await SocialService.GetFacebookPages(account.AccessToken, endPoint);

                        //if (FacebookProfile != null)
                        //{
                        //    PageId = FacebookProfile[0].Id;
                        //}
                    }
                }

            }

            else
            {
                NavigationManager.NavigateTo("/Portal/Social/Facebook/Connect");
            }


        }

        protected async Task CreatePost()
        {

            string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

            string pageId = "102420827994118";

            string token = "EAAQGsEzdprgBAJDGUx6W8PvqpBwL79rNYNVUKMTo6HTs4auWovToZBpkveMW56l1mtZA1ZB60j0WtT6udR2s6mUHQeDjXhvnVDoxznVWj1DnZCP4jkVVGXXC8ugvblCaSjs6XLOWZAyZAdZCKJUu0O48YZC3KbPFVU3ZBwy1t0EtjKQTn0dMarJ9PTxTyQgFwqe0ZD";


            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("message", "Hello Fans!"),
                new KeyValuePair<string, string>("access_token", token),
            });



            var result = await SocialService.CreatePost(pageId, endPoint, content);

            if (result != null)
            {
                NavigationManager.NavigateTo("/Portal/Social/Facebook/Posts");
            }
        }
    }
}
