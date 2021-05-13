using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Sohi.Models;
using Sohi.Web.Models;
using Sohi.Web.Services.Social;

namespace Sohi.Web.Shared
{
    public class SocialLayoutBase : LayoutComponentBase
    {

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        [Inject]
        public UserManager<User> userManager { get; set; }

        public User user { get; set; }

        [Inject]
        public ISocialService SocialService { get; set; }

        public string FacebookAccessToken { get; set; }

        [Inject]
        public IConfiguration _config { get; set; }

        public bool flag { get; set; } = true;

        public List<Profile> FacebookProfile { get; set; }

        public List<Profile> InstagramProfile { get; set; }

        public string PageId { get; set; }

        public string PageToken { get; set; }


        protected override async Task OnParametersSetAsync()
        {
            var authState = await authenticationStateTask;

            if (authState.User.Identity.IsAuthenticated)
            {
                user = await userManager.GetUserAsync(authState.User);
            }

            List<SocialMedia> accounts = await SocialService.GetAllTokens(user.AccountId.ToString());

            if (accounts != null && accounts.Count != 0)
            {
                foreach (var account in accounts)
                {
                    if (account.Type == "FACEBOOK")
                    {

                        string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

                        FacebookAccessToken = account.AccessToken;

                        FacebookProfile = await SocialService.GetFacebookPages(account.AccessToken, endPoint);


                        var pagetoken = await SocialService.GenerateFacebookPageTokenAsync("102420827994118", account.AccessToken, endPoint);

                        if (pagetoken != null)
                        {
                            InstagramProfile = await SocialService.GetInstagramAccounts(pagetoken, endPoint);

                        }

                    }
                }

            }

            else
            {
                flag = false;
                //NavigationManager.NavigateTo("/Portal/Social/Facebook/Connect");
            }


        }


        protected void PageSelectionChanged(SocialMedia socialMedia)
        {
            PageId = socialMedia.AccountId;
            PageToken = socialMedia.AccessToken;
        }


    }
}
