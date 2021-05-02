using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
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


        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        [Inject]
        public UserManager<User> userManager { get; set; }

        public User user { get; set; }

        protected async override Task OnInitializedAsync()
        {
            var authState = await authenticationStateTask;

            if (authState.User.Identity.IsAuthenticated)
            {
                user = await userManager.GetUserAsync(authState.User);
            }

            if (LeadId != null)
            {
                PageHeader = "Edit Lead";

                Guid leadId = Guid.Parse(LeadId);

                Guid accountid = Guid.Parse(user.AccountId);

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
                    AccountId = Guid.Parse(user.AccountId),
                    CreatedBy = user.FirstName + " " + user.LastName,
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
            //Guid accountid = Guid.Parse("7458fd55-4b47-434b-9a68-613f4ca9a059");

            await LeadService.DeleteLead(Lead.LeadId, Lead.AccountId);
            NavigationManager.NavigateTo("/Portal/Leads/LeadList");
        }
    }
}
