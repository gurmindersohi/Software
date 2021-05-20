using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Sohi.Models;
using Sohi.Web.Models;
using Sohi.Web.Services.Social;

namespace Sohi.Web.Pages.Portal.Social
{
    public class CreateBase : ComponentBase
    {

        public Post Post { get; set; } = new Post();

        [Inject]
        public ISocialService SocialService { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IConfiguration _config { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        [Inject]
        public UserManager<User> userManager { get; set; }

        public User user { get; set; }


        public bool flag { get; set; } = false;

        public List<Profile> TotalAccounts { get; set; } = new List<Profile>();

        //public List<Profile> InstagramProfile { get; set; } = new List<Profile>();

        public Profile Profile { get; set; } = new Profile();


        public List<Profile> SelectedProfiles { get; set; } = new List<Profile>();


        protected override async Task OnInitializedAsync()
        {
            var authenticateState = await authenticationStateTask;

            if (!authenticateState.User.Identity.IsAuthenticated)
            {
            }

            user = await userManager.GetUserAsync(authenticateState.User);



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

                            var profile = await SocialService.GetFacebookPage(account.AccessToken, endPoint);

                            if (profile != null)
                            {
                                profile.Token = account.AccessToken;
                                profile.Type = account.Type;
                                TotalAccounts.Add(profile);

                                SelectedProfiles.Add(profile);

                            }

                        }

                        if (account.Type == "Instagram")
                        {

                            string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

                            var profile = await SocialService.GetInstagramBusinessAccountDetails(account.PageId, account.AccessToken, endPoint);

                            if (profile != null)
                            {
                                profile.Token = account.AccessToken;
                                profile.Type = account.Type;
                                TotalAccounts.Add(profile);

                                SelectedProfiles.Add(profile);

                            }

                        }
                    }

                    //SelectedProfiles = TotalAccounts;

                    StateHasChanged();

                }

                else
                {
                    flag = true;
                    StateHasChanged();
                    //NavigationManager.NavigateTo("/Portal/Social/Facebook/Connect");
                }
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


        protected async Task CreatePost()
        {


            if (SelectedProfiles.Count != 0)
            {

                foreach (var profile in SelectedProfiles)
                {
                    string pageId = profile.Id;

                    string endPoint = _config.GetSection("FacebookApp").GetSection("EndPoint").Value;

                    string token = profile.Token;


                    try
                    {
                        if (profile.Type == "Facebook")
                        {
                            var content = new FormUrlEncodedContent(new[]
                            {
                                new KeyValuePair<string, string>("message", "Hello Fans!"),
                                new KeyValuePair<string, string>("access_token", token),
                            });

                            var result = await SocialService.CreatePost(pageId, endPoint, content);
                        }

                        else if (profile.Type == "Instagram")
                        {
                            var content = new FormUrlEncodedContent(new[]
                            {
                                new KeyValuePair<string, string>("image_url", "https://sohi.blob.core.windows.net/software/Accounts/DC/DC.png"),
                                new KeyValuePair<string, string>("caption", "Test!"),
                                new KeyValuePair<string, string>("access_token", token),
                            });

                            var conatinerId = await SocialService.CreateInstagramPostContainer(pageId, endPoint, content);

                            if (conatinerId != null)
                            {
                                var post = new FormUrlEncodedContent(new[]
                                {
                                    new KeyValuePair<string, string>("creation_id", conatinerId),
                                    new KeyValuePair<string, string>("access_token", token),
                                });

                                var result = await SocialService.CreateInstagramPost(pageId, endPoint, post);
                            }
                        }
                    }

                    catch (Exception)
                    {

                    }

                }


            }

            //if (result != null)
            //{
            //    NavigationManager.NavigateTo("/Portal/Social/Facebook/Posts");
            //}
        }

    }
}
