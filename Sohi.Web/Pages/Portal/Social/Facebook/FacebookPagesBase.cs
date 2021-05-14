using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Sohi.Web.Pages.Portal.Social.Facebook
{
    public class FacebookPagesBase : ComponentBase
    {
        protected bool ShowConfirmation { get; set; }

        public void Show()
        {
            ShowConfirmation = true;
            StateHasChanged();
        }


        protected async Task OnConfirmationChange(bool value)
        {
            ShowConfirmation = false;
            //await ConfirmationChanged.InvokeAsync(value);
        }
    }
}
