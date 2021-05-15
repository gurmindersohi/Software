using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using Sohi.Models;
using Sohi.Web.Models;
using Sohi.Web.Pages.Portal.Social.Facebook;
using Sohi.Web.Services.Social;

namespace Sohi.Web.Pages.Portal.Settings
{
    public class ConnectBase : ComponentBase
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public ISocialService SocialService { get; set; }

        [Inject]
        public UserManager<User> userManager { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        public User user { get; set; }

        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        [Inject]
        public IConfiguration _config { get; set; }

        public List<Profile> Profiles { get; set; }


        public List<Profile> InstagramProfiles { get; set; }

        public bool SelectPage { get; set; } = false;

        public List<Profile> SelectedProfiles { get; set; } = new List<Profile>();

        protected override async Task OnParametersSetAsync()
        {
            var authState = await authenticationStateTask;

            if (authState.User.Identity.IsAuthenticated)
            {
                user = await userManager.GetUserAsync(authState.User);
            }

        }




        protected FacebookPages facebookPages { get; set; }

        protected void ShowFacebookPages()
        {
            facebookPages.Show();
        }



        protected async void ConnectFacebook()
        {

            var response = await JSRuntime.InvokeAsync<string>(identifier: "LoginDialog");

            if (response != null)
            {
                string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;
                string client_id = _config.GetSection("FacebookApp").GetSection("ClientId").Value;
                string client_secret = _config.GetSection("FacebookApp").GetSection("ClientSecret").Value;

                string longLivedUserToken = await SocialService.LongLivedUserToken(client_id, client_secret, endPoint, response);

                Profiles = await SocialService.GetFacebookPages(longLivedUserToken, endPoint);

                StateHasChanged();
            }

        }

        protected async void ConnectInstagram()
        {

            List<Profile> instagramAccounts = new List<Profile>();

            var response = await JSRuntime.InvokeAsync<string>(identifier: "LoginDialog");

            if (response != null)
            {
                string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;
                string client_id = _config.GetSection("FacebookApp").GetSection("ClientId").Value;
                string client_secret = _config.GetSection("FacebookApp").GetSection("ClientSecret").Value;

                string longLivedUserToken = await SocialService.LongLivedUserToken(client_id, client_secret, endPoint, response);

                var facebookPages = await SocialService.GetFacebookPages(longLivedUserToken, endPoint);

                foreach (var facebookPage in facebookPages)
                {
                    var accounts = await SocialService.GetInstagramAccounts(facebookPage.Token, endPoint);

                    if (accounts.Count != 0)
                    {
                        foreach (var account in accounts)
                        {
                            account.Token = facebookPage.Token;
                            instagramAccounts.Add(account);
                        }
                    }

                }

                InstagramProfiles = instagramAccounts;

                StateHasChanged();
            }

        }


        protected void CheckboxClicked(Profile profile)
        {

            var item = (SelectedProfiles.Find(p => p.Id == profile.Id));

            if (item == null)
            {
                SelectedProfiles.Add(profile);
            }
            else
            {
                SelectedProfiles.Remove(item);
            }
        }

        protected async void ConnectFacebookPages()
        {

            var pages = SelectedProfiles;

            try
            {
                if (pages != null)
                {
                    string platform = "Facebook";

                    foreach (var page in pages)
                    {
                        SocialMedia result = await SaveToken(page.Token, page.Id, page.Name, page.Image, platform);
                    }

                    NavigationManager.NavigateTo("/Portal/Settings/Accounts");
                }
            }

            catch
            {

            }

        }

        protected async void ConnectInstagramAccounts()
        {

            var pages = SelectedProfiles;

            try
            {
                if (pages != null)
                {
                    string platform = "Instagram";

                    foreach (var page in pages)
                    {
                        SocialMedia result = await SaveToken(page.Token, page.Id, page.Name, page.Image, platform);
                    }

                    NavigationManager.NavigateTo("/Portal/Settings/Accounts");
                }
            }

            catch
            {

            }

        }



        protected async Task<SocialMedia> SaveToken(string token, string pageId, string name, string image, string platform)
        {

            SocialMedia result = null;

            var account = new SocialMedia
            {
                Id = Guid.NewGuid(),
                PageId = pageId,
                Name = name,
                Image = image,
                Type = platform,
                AccessToken = token,
                CreatedOn = DateTime.Now,
                TokenExpiryDate = DateTime.Now.AddDays(90),
                Email = user.Email,
                UserId = user.Id,
                AccountId = user.AccountId

            };

            result = await SocialService.SaveToken(account);


            return result;
        }



    }
}
