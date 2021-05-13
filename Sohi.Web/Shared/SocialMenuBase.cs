using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Sohi.Models;
using Sohi.Web.Services.Social;

namespace Sohi.Web.Shared
{
    public class SocialMenuBase : LayoutComponentBase
    {
        [Parameter]
        public List<Profile> FacebookProfile { get; set; }

        [Parameter]
        public List<Profile> InstagramProfile { get; set; }

        [Parameter]
        public EventCallback<SocialMedia> OnPageSelection { get; set; }

        [Inject]
        public ISocialService SocialService { get; set; }

        [Inject]
        public IConfiguration _config { get; set; }

        [Parameter]
        public string FacebookAccessToken { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        protected bool collapseNavMenu = true;

        protected string NavMenuCssClass => collapseNavMenu ? "collapse" : null;

        protected void ToggleNavMenu()
        {
            collapseNavMenu = !collapseNavMenu;
        }

        protected async Task ReturnPageId(string Id)
        {
            SocialMedia socialMedia = new SocialMedia();

            socialMedia.AccountId = Id;

            string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

            var pagetoken = await SocialService.GenerateFacebookPageTokenAsync(Id, FacebookAccessToken, endPoint);

            if (pagetoken != null)
            {
                socialMedia.AccessToken = pagetoken;

                await OnPageSelection.InvokeAsync(socialMedia);

                NavigationManager.NavigateTo(NavigationManager.Uri);
            }

        }

    }
}
