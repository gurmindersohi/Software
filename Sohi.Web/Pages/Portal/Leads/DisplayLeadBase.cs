using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Sohi.Models;
using Sohi.Web.Services.Leads;
using Sohi.Web.Shared;

namespace Sohi.Web.Pages.Portal.Leads
{
    public class DisplayLeadBase : ComponentBase
    {
        [Parameter]
        public EventCallback<Guid> OnLeadDeleted { get; set; }

        [Inject]
        public ILeadService LeadService { get; set; }

        [Parameter]
        public Lead Lead { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        protected ConfirmBase DeleteConfirmation { get; set; }

        protected void Delete_Click()
        {
            DeleteConfirmation.Show();
        }

        protected async Task ConfirmDelete_Click(bool deleteConfirmed)
        {
            if (deleteConfirmed)
            {
                Guid accountid = Guid.Parse("7458fd55-4b47-434b-9a68-613f4ca9a059");

                await LeadService.DeleteLead(Lead.LeadId, accountid);
                await OnLeadDeleted.InvokeAsync(Lead.LeadId);
            }
        }
    }
}
