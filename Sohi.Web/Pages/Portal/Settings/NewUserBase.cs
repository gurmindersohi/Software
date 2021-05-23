using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using Sohi.Models;
using Sohi.Web.Models;
using Sohi.Web.Pages.Portal.Social.Facebook;
using Sohi.Web.Services.Accounts;
using Sohi.Web.Services.Social;
using Microsoft.AspNetCore.Http.Extensions;

namespace Sohi.Web.Pages.Portal.Settings
{
    public class NewUserBase : ComponentBase
    {
        public NewUserModel NewUserModel { get; set; } = new NewUserModel();

        [Inject]
        public UserManager<User> userManager { get; set; }

        //[Inject]
        //public IEmailSender EmailSender { get; set; }

        [Inject]
        public RoleManager<IdentityRole> roleManager { get; set; }

        public IEnumerable<User> Users { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        public User User { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        protected bool ShowConfirmation { get; set; } = false;

        protected override async Task OnParametersSetAsync()
        {

            var authState = await authenticationStateTask;

            if (authState.User.Identity.IsAuthenticated)
            {
                User = await userManager.GetUserAsync(authState.User);
            }

        }

        protected async Task HandleValidSubmit()
        {
            var user = new User
            {
                UserName = NewUserModel.Email,
                Email = NewUserModel.Email,
                AccountId = User.AccountId.ToString(),
                EmailConfirmed = true
                
            };

            var result = await userManager.CreateAsync(user, NewUserModel.Password);

            if (result.Succeeded)
            {
                var role = await roleManager.FindByIdAsync("63c3f83d-99bd-4909-890f-5b455dc8de25");

                var assigned = await userManager.AddToRoleAsync(user, role.Name);

                NavigationManager.NavigateTo("/Portal/Settings/ManageUsers");
            }

        }
    }
}
