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

namespace Sohi.Web.Pages.Portal.Social.Instagram
{
    public class InstagramLayoutBase : LayoutComponentBase
    {

        public List<Profile> InstagramProfile { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        [Inject]
        public UserManager<User> userManager { get; set; }

        public User user { get; set; }

        public string PageId { get; set; }

        public string AccessToken { get; set; }

        public bool flag { get; set; } = true;

        [Inject]
        public IConfiguration _config { get; set; }

        [Inject]
        public ISocialService SocialService { get; set; }


        protected override async Task OnInitializedAsync()
        {
            var authState = await authenticationStateTask;

            if (authState.User.Identity.IsAuthenticated)
            {
                user = await userManager.GetUserAsync(authState.User);
            }


            string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

                AccessToken = "EAAQGsEzdprgBAN4V6NkJCLWQxJ6Gy6HHfPTf09oZAF5T5OmTbBZAogvpUimMOMz7IGiEPUZBIZCxV0EiEEqO3xjoCJb4lUEdu3ZARZBUe11MXeINzhOrQq5RtT6QxpTq1v4e1QMNZCBvRZCqUNX3SyJ6jhGPasHlKVCseuBDkY41pFqHYJVrBVIOpCKwBNzpt6oB6LCCgMZCse1MWqT7cwHJo";
                InstagramProfile = await SocialService.GetInstagramAccounts(AccessToken, endPoint);

                if (InstagramProfile != null)
                {
                    PageId = InstagramProfile[0].Id;
                }




        }
    }
}
