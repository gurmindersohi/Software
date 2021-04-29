using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Sohi.Models;
using Sohi.Web.Services.Leads;

namespace Sohi.Web.Pages.Portal.Leads
{
    public class LeadDetailsBase : ComponentBase
    {
        public Lead Lead { get; set; } = new Lead();

        [Inject]
        public ILeadService LeadService { get; set; }

        [Parameter]
        public string LeadId { get; set; }

        protected async override Task OnInitializedAsync()
        {
            Guid leadId = Guid.Parse(LeadId);

            Guid accountid = Guid.Parse("7458fd55-4b47-434b-9a68-613f4ca9a059");


            Lead = await LeadService.GetLead(leadId, accountid);
        }
    }
}
