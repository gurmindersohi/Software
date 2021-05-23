using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using Sohi.Models;
using Sohi.Web.Models;
using Sohi.Web.Pages.Portal.Social.Facebook;
using Sohi.Web.Services.Accounts;
using Sohi.Web.Services.Social;
using Sohi.Web.Shared;

namespace Sohi.Web.Pages.Portal.Settings
{
    public class ManageUsersBase : ComponentBase
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public IAccountService AccountService { get; set; }

        [Inject]
        public UserManager<User> userManager { get; set; }

        [Inject]
        public RoleManager<IdentityRole> roleManager { get; set; }

        public IEnumerable<User> Users { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        public User user { get; set; }

        public IList<string> Role { get; set; }

        public bool flag { get; set; } = false;


        protected ConfirmBase DeleteConfirmation { get; set; }


        protected override async Task OnParametersSetAsync()
        {

            var authState = await authenticationStateTask;

            if (authState.User.Identity.IsAuthenticated)
            {
                user = await userManager.GetUserAsync(authState.User);
            }

            GetUsers();

        }

        protected void GetUsers()
        {
            Users = userManager.Users.Where(a => a.AccountId == user.AccountId.ToString());
        }

    }
}
