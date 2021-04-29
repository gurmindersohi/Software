using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Components;
using Sohi.Models;
using Sohi.Web.Models;
using Sohi.Web.Services.Leads;
namespace Sohi.Web.Pages.Portal.Leads
{
    public class EditLeadBase : ComponentBase
    {
        [Inject]
        public ILeadService LeadService { get; set; }

        public string PageHeader { get; set; }

        private Lead Lead { get; set; } = new Lead();

        public EditLeadModel EditLeadModel { get; set; } = new EditLeadModel();

        [Inject]
        public IMapper Mapper { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Parameter]
        public string LeadId { get; set; }

        protected async override Task OnInitializedAsync()
        {
            if (LeadId != null)
            {
                PageHeader = "Edit Lead";

                Guid leadId = Guid.Parse(LeadId);

                Guid accountid = Guid.Parse("7458fd55-4b47-434b-9a68-613f4ca9a059");

                Lead = await LeadService.GetLead(leadId, accountid);
            }
            else
            {
                PageHeader = "Create Lead";
            }

            Mapper.Map(Lead, EditLeadModel);
        }

        protected async Task HandleValidSubmit()
        {

            Mapper.Map(EditLeadModel, Lead);
            Lead result = null;

            if (Lead.LeadId != Guid.Empty)
            {
                result = await LeadService.UpdateLead(Lead);
            }
            else
            {
                var lead = new Lead
                {
                    LeadId = Guid.NewGuid(),
                    FirstName = Lead.FirstName,
                    LastName = Lead.LastName,
                    FullName = Lead.FirstName + " " + Lead.LastName,
                    Email = Lead.Email,
                    AccountId = Guid.Parse("7458fd55-4b47-434b-9a68-613f4ca9a059"),
                    CreatedBy = "Gurminder",
                    CreatedOn = DateTime.Now,
                    DateOfBirth = Lead.DateOfBirth,
                    IsActive = true
                };

                result = await LeadService.CreateLead(lead);
            }

            if (result != null)
            {
                NavigationManager.NavigateTo("/Portal/Leads/LeadList");
            }
        }

        protected async Task Delete_Click()
        {
            Guid accountid = Guid.Parse("7458fd55-4b47-434b-9a68-613f4ca9a059");

            await LeadService.DeleteLead(Lead.LeadId, accountid);
            NavigationManager.NavigateTo("/Portal/Leads/LeadList");
        }
    }
}
