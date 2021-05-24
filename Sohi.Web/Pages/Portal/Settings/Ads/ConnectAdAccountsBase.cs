using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using Sohi.Models;
using Sohi.Web.Models;
using Sohi.Web.Pages.Portal.Social.Facebook;
using Sohi.Web.Services.Ads;
using Sohi.Web.Services.Social;

namespace Sohi.Web.Pages.Portal.Settings.Ads
{
    public class ConnectAdAccountsBase : ComponentBase
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public UserManager<User> userManager { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        public User user { get; set; }


        [Inject]
        public IConfiguration _config { get; set; }


        protected override async Task OnParametersSetAsync()
        {
            var authState = await authenticationStateTask;

            if (authState.User.Identity.IsAuthenticated)
            {
                user = await userManager.GetUserAsync(authState.User);
            }
        }
       
        protected void ConnectFacebook()
        {
            string version = _config.GetSection("FacebookApp").GetSection("version").Value;

            string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;
            string client_id = _config.GetSection("FacebookApp").GetSection("ClientId").Value;
            string client_secret = _config.GetSection("FacebookApp").GetSection("ClientSecret").Value;
            string RedirectURL = _config.GetSection("FacebookApp").GetSection("AdsRedirectURL").Value;

            string socialScopes = _config.GetSection("FacebookApp").GetSection("SocialScopes").Value;

            string url = string.Format("https://www.facebook.com/v" + version + "/dialog/oauth?client_id={0}&redirect_uri={1}&scope={2}", client_id, RedirectURL, socialScopes);

            NavigationManager.NavigateTo(url);


        }

       

    }
}
