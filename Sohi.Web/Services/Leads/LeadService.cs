using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Sohi.Models;

namespace Sohi.Web.Services.Leads
{
    public class LeadService :ILeadService
    {
        private readonly HttpClient httpClient;

        public LeadService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<IEnumerable<Lead>> GetLeads()
        {
            return await httpClient.GetJsonAsync<Lead[]>("api/leads/7458fd55-4b47-434b-9a68-613f4ca9a059");
        }
    }
}
