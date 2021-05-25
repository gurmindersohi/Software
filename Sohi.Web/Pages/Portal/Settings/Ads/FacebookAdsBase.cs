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
using Sohi.Web.Services.Ads;
using Sohi.Web.Services.Social;

namespace Sohi.Web.Pages.Portal.Settings.Ads
{
    public class FacebookAdsBase : ComponentBase
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public ISocialService SocialService { get; set; }

        [Inject]
        public IAdAccountService AdAccountService { get; set; }

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

            if (code != null)
            {
                string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;
                string client_id = _config.GetSection("FacebookApp").GetSection("ClientId").Value;
                string client_secret = _config.GetSection("FacebookApp").GetSection("ClientSecret").Value;
                string RedirectURL = _config.GetSection("FacebookApp").GetSection("AdsRedirectURL").Value;


                string userToken = await SocialService.GenerateFacebookTokenAsync(client_id, client_secret, endPoint, RedirectURL, code);

                string longLivedUserToken = await SocialService.LongLivedUserToken(client_id, client_secret, endPoint, userToken);

                Profiles = await AdAccountService.GetFacebookAdAccounts(longLivedUserToken, endPoint);

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

        protected async void ConnectFacebookAdAccounts()
        {

            var accounts = SelectedProfiles;

            try
            {
                if (accounts != null)
                {
                    string platform = "Facebook";

                    foreach (var account in accounts)
                    {
                        AdAccount result = await SaveToken(account.Token, account.Id, account.Name, account.Image, platform);
                    }

                    NavigationManager.NavigateTo("/Portal/Settings/Ads/Accounts");
                }
            }

            catch
            {

            }

        }


        protected async Task<AdAccount> SaveToken(string token, string userAccountId, string name, string image, string platform)
        {

            AdAccount result = null;

            var account = new AdAccount
            {
                Id = Guid.NewGuid(),
                UserAccountId = userAccountId,
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

            result = await AdAccountService.SaveAccount(account);


            return result;
        }



    }
}
