using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Sohi.Models;
using Sohi.Web.Models;
using Sohi.Web.Services.Social;

namespace Sohi.Web.Pages.Portal.Settings
{
    public class InstagramBase : ComponentBase
    {

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public ISocialService SocialService { get; set; }

        [Inject]
        public IConfiguration _config { get; set; }

        public List<Profile> Profiles { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        public User user { get; set; }

        [Inject]
        public UserManager<User> userManager { get; set; }

        public List<Profile> SelectedProfiles { get; set; } = new List<Profile>();

        protected override async Task OnParametersSetAsync()
        {
            var authState = await authenticationStateTask;

            if (authState.User.Identity.IsAuthenticated)
            {
                user = await userManager.GetUserAsync(authState.User);
            }

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
                if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("code", out var _code))
                {
                    await GenerateFacebookTokenFromCode(_code);
                }
            }
        }

        protected async Task GenerateFacebookTokenFromCode(string code)
        {

            List<Profile> instagramAccounts = new List<Profile>();

            if (code != null)
            {
                string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;
                string client_id = _config.GetSection("FacebookApp").GetSection("ClientId").Value;
                string client_secret = _config.GetSection("FacebookApp").GetSection("ClientSecret").Value;
                string RedirectURL = _config.GetSection("FacebookApp").GetSection("InstagramRedirectURL").Value;


                string userToken = await SocialService.GenerateFacebookTokenAsync(client_id, client_secret, endPoint, RedirectURL, code);

                string longLivedUserToken = await SocialService.LongLivedUserToken(client_id, client_secret, endPoint, userToken);

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

                Profiles = instagramAccounts;

                StateHasChanged();
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

        protected async Task<SocialMedia> SaveToken(string token, string pageId, string name, string image, string platform)
        {
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

            SocialMedia result = await SocialService.SaveToken(account);
            return result;
        }


    }
}
