using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Sohi.Web.Models;
using Sohi.Web.Shared;

namespace Sohi.Web.Pages.Portal.Settings
{
    public class DisplayUserBase : ComponentBase
    {
        [Parameter]
        public EventCallback<string> OnUserDeleted { get; set; }


        [Parameter]
        public User user { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        protected ConfirmBase DeleteConfirmation { get; set; }

        [Inject]
        public UserManager<User> userManager { get; set; }

        protected void Delete_Click()
        {
            DeleteConfirmation.Show();
        }

        protected async Task DeleteAccount(User deletedUser)
        {
            DeleteConfirmation.Show();
        }


        protected async Task ConfirmDelete_Click(bool deleteConfirmed)
        {
            if (deleteConfirmed)
            {

                await userManager.DeleteAsync(user);
                await OnUserDeleted.InvokeAsync(user.Id);

            }
        }
    }
}
