using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Sohi.Models;
using Sohi.Web.Models;
using Sohi.Web.Services.Settings;

namespace Sohi.Web.Pages.Portal.Settings
{
    public class GeneralBase : ComponentBase
    {

        [Inject]
        public ISettingsService SettingsService { get; set; }

        //public User User { get; set; } = new User();

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        [Inject]
        public UserManager<User> userManager { get; set; }

        public User user { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        protected async override Task OnInitializedAsync()
        {
            var authState = await authenticationStateTask;

            if (authState.User.Identity.IsAuthenticated)
            {
                user = await userManager.GetUserAsync(authState.User);
            }
          
        }

        protected async Task HandleValidSubmit()
        {

            var result = await userManager.UpdateAsync(user);


            if (result != null)
            {
                NavigationManager.NavigateTo("/Portal/Settings/General");
            }

        }
    }
}
