using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Sohi.Models;
using Sohi.Web.Models;
using Sohi.Web.Services.Leads;

namespace Sohi.Web.Pages.Portal.Leads
{
    public class LeadListBase : ComponentBase
    {
        [Inject]
        public ILeadService LeadService { get; set; }

        public IEnumerable<Lead> Leads { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        [Inject]
        public UserManager<User> userManager { get; set; }

        public User user { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var authState = await authenticationStateTask;

            if (authState.User.Identity.IsAuthenticated)
            {
                user = await userManager.GetUserAsync(authState.User);
            }

            Guid accountid = Guid.Parse(user.AccountId);

            Leads = (await LeadService.GetLeads(accountid)).ToList();
        }

        protected async Task LeadDeleted()
        {

            Guid accountid = Guid.Parse(user.AccountId);

            Leads = (await LeadService.GetLeads(accountid)).ToList();
        }
    }
}
