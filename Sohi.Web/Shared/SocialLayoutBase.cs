using System;
using System.Collections.Generic;
using System.Net;
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

        [Inject]
        public IConfiguration _config { get; set; }

        public List<Profile> FacebookProfile { get; set; } = new List<Profile>();

        public List<Profile> InstagramProfile { get; set; } = new List<Profile>();

        public Profile Profile { get; set; } = new Profile();


        public string PageId { get; set; }

        public string PageToken { get; set; }


        protected override async Task OnInitializedAsync()
        {
            try
            {
                var authenticateState = await authenticationStateTask;

                if (!authenticateState.User.Identity.IsAuthenticated)
                {
                }

                var result = await userManager.GetUserAsync(authenticateState.User);

                if (result != null)
                {
                    user = result;
                }
            }
            catch (Exception ex)
            {
                
            }

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                List<SocialMedia> accounts = await SocialService.GetAllTokens(user.AccountId.ToString());

                if (accounts != null && accounts.Count != 0)
                {
                    foreach (var account in accounts)
                    {
                        if (account.Type == "Facebook")
                        {

                            string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

                            Profile = await SocialService.GetFacebookPage(account.AccessToken, endPoint);

                            if (Profile != null)
                            {
                                Profile.Token = account.AccessToken;
                                FacebookProfile.Add(Profile);

                            }

                        }

                        if (account.Type == "Instagram")
                        {

                            string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

                            Profile = await SocialService.GetInstagramBusinessAccountDetails(account.PageId, account.AccessToken, endPoint);

                            if (Profile != null)
                            {
                                Profile.Token = account.AccessToken;
                                InstagramProfile.Add(Profile);

                            }

                        }
                    }

                    StateHasChanged();

                }

                else
                {
                    //flag = false;
                    //NavigationManager.NavigateTo("/Portal/Social/Facebook/Connect");
                }
            }
        }


        protected void PageSelectionChanged(Profile profile)
        {
            Profile = profile;
        }


    }
}
