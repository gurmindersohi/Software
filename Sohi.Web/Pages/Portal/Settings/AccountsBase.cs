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
    public class AccountsBase : ComponentBase
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

        public List<SocialMedia> Accounts { get; set; }

        public bool SelectPage { get; set; } = false;

        public List<Profile> SelectedProfiles { get; set; } = new List<Profile>();

        public bool flag { get; set; } = false;

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
                Accounts = accounts;

            }

            else
            {
                flag = true;
            }

        }



    }
}
