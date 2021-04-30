using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Sohi.Models;
using Sohi.Web.Services.Leads;

namespace Sohi.Web.Pages.Portal.Leads
{
    public class LeadListBase : ComponentBase
    {
        [Inject]
        public ILeadService LeadService { get; set; }

        public IEnumerable<Lead> Leads { get; set; }

        protected override async Task OnInitializedAsync()
        {

            Guid accountid = Guid.Parse("7458fd55-4b47-434b-9a68-613f4ca9a059");

            Leads = (await LeadService.GetLeads(accountid)).ToList();
        }

        protected async Task LeadDeleted()
        {
            Guid accountid = Guid.Parse("7458fd55-4b47-434b-9a68-613f4ca9a059");

            Leads = (await LeadService.GetLeads(accountid)).ToList();
        }
    }
}
