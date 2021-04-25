using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
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

            Leads = (await LeadService.GetLeads()).ToList();
        }
    }
}
