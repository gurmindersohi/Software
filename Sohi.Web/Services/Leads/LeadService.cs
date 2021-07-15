using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Sohi.Models;

namespace Sohi.Web.Services.Leads
{
    public class LeadService : ILeadService
    {
        private readonly HttpClient httpClient;

        public LeadService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<Lead> CreateLead(Lead newlead)
        {
            return await httpClient.PostJsonAsync<Lead>("api/Leads", newlead);
        }

        public async Task DeleteLead(Guid leadId, Guid accountid)
        {
            await httpClient.DeleteAsync($"api/leads/{leadId}/{accountid}");
        }

        public async Task<Lead> GetLead(Guid leadId, Guid accountid)
        {
            return await httpClient.GetJsonAsync<Lead>($"api/leads/{leadId}/{accountid}");
        }

        public async Task<IEnumerable<Lead>> GetLeads(Guid accountid)
        {
            return await httpClient.GetJsonAsync<Lead[]>($"api/leads/{accountid}");
        }

        public async Task<Lead> UpdateLead(Lead updatedlead)
        {
            return await httpClient.PutJsonAsync<Lead>("api/leads/", updatedlead);
        }
    }
}
