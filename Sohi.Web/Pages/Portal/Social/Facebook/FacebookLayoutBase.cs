using System;
using Microsoft.AspNetCore.Components;
using Sohi.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using Sohi.Web.Services.Social;
using Sohi.Web.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Sohi.Web.Pages.Portal.Social.Facebook
{
    public class FacebookLayoutBase : LayoutComponentBase
    {
        private bool collapseNavMenu = true;

        protected string NavMenuCssClass => collapseNavMenu ? "collapse" : null;

        protected void ToggleNavMenu()
        {
            collapseNavMenu = !collapseNavMenu;
        }

        //[Inject]
        //public IConfiguration _config { get; set; }

        //[Inject]
        //public NavigationManager NavigationManager { get; set; }

        //[Inject]
        //public ISocialService SocialService { get; set; }

        //[Inject]
        //public IJSRuntime JSRuntime { get; set; }

        //public List<Profile> FacebookProfile { get; set; }

        //[CascadingParameter]
        //private Task<AuthenticationState> authenticationStateTask { get; set; }

        //[Inject]
        //public UserManager<User> userManager { get; set; }

        //public User user { get; set; }

        //public string PageId { get; set; }

        //public string AccessToken { get; set; }

        //public bool flag { get; set; } = true;


        [CascadingParameter(Name = "Id")]
        public string Id { get; set; }

        //[CascadingParameter(Name = "FacebookAccessToken")]
        //public string FacebookAccessToken { get; set; }

        //[Parameter]
        //public EventCallback<string> PageSelection { get; set; }

        //protected override async Task OnInitializedAsync()
        //{
        //    var authState = await authenticationStateTask;

        //    if (authState.User.Identity.IsAuthenticated)
        //    {
        //        user = await userManager.GetUserAsync(authState.User);
        //    }


        //    string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;


        //    SocialMedia account = await SocialService.GetToken(user.AccountId.ToString(), "FACEBOOK");

        //    if (account != null)
        //    {
        //        //FacebookProfile = await SocialService.GetFacebookAccountAsync(account.AccessToken, endPoint);

        //        AccessToken = account.AccessToken;
        //        FacebookProfile = await SocialService.GetFacebookPages(account.AccessToken, endPoint);

        //        if (FacebookProfile != null)
        //        {
        //            PageId = FacebookProfile[0].Id;
        //        }

        //    }

        //    else
        //    {
        //        flag = false;
        //        //NavigationManager.NavigateTo("/Portal/Social/Facebook/Connect");
        //    }


        //}

        //protected async Task OnPageSelection(ChangeEventArgs e)
        //{
        //    PageId = e.Value.ToString();

        //    //await PageSelection.InvokeAsync(PageId);
        //    //StateHasChanged();

        //    NavigationManager.NavigateTo(NavigationManager.Uri);


        //}


        //protected async void ConnectFacebook()
        //{

        //    var response = await JSRuntime.InvokeAsync<string>(identifier: "LoginDialog");


        //    string platform = "FACEBOOK";

        //    SocialMedia result = await SaveToken(response, platform);


        //    if (result != null)
        //    {
        //        NavigationManager.NavigateTo("/Portal/Social/Facebook");
        //    }

        //}


        //protected async Task<SocialMedia> SaveToken(string response, string platform)
        //{

        //    SocialMedia result = null;

        //    var account = new SocialMedia
        //    {
        //        Id = Guid.NewGuid(),
        //        Type = platform,
        //        AccessToken = response,
        //        CreatedOn = DateTime.Now,
        //        TokenExpiryDate = DateTime.Now.AddDays(90),
        //        Email = user.Email,
        //        UserId = user.Id,
        //        AccountId = user.AccountId

        //    };

        //        result = await SocialService.SaveToken(account);


        //    return result;
        //}

    }
}
