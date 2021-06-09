using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Sohi.Web.Shared
{
    public class SuccessModalBase : ComponentBase
    {
        protected bool ShowConfirmation { get; set; }

        [Parameter]
        public string ConfirmationTitle { get; set; } = "Success";

        [Parameter]
        public string ConfirmationMessage { get; set; } = "Your post has been successfully posted!";

        public void Show()
        {
            ShowConfirmation = true;
            StateHasChanged();
        }

        [Parameter]
        public EventCallback<bool> ConfirmationChanged { get; set; }

        protected async Task OnConfirmationChange(bool value)
        {
            ShowConfirmation = false;
            await ConfirmationChanged.InvokeAsync(value);
        }

    }
}
