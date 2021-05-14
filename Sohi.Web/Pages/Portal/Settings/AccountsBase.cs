using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;
using Sohi.Models;
using Sohi.Web.Models;
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

        protected override async Task OnParametersSetAsync()
        {
            var authState = await authenticationStateTask;

            if (authState.User.Identity.IsAuthenticated)
            {
                user = await userManager.GetUserAsync(authState.User);
            }

        }

        protected async void ConnectFacebook()
        {

            var response = await JSRuntime.InvokeAsync<string>(identifier: "LoginDialog");


            string platform = "FACEBOOKPAGE";

            SocialMedia result = await SaveToken(response, platform);


            if (result != null)
            {
                NavigationManager.NavigateTo("/Portal/Settings/Accounts");
            }

        }

        protected async Task<SocialMedia> SaveToken(string response, string platform)
        {

            SocialMedia result = null;

            var account = new SocialMedia
            {
                Id = Guid.NewGuid(),
                Type = platform,
                AccessToken = response,
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
