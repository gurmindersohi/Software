using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sohi.Models;

namespace Sohi.Web.Services.Leads
{
    public interface ILeadService
    {
        Task<IEnumerable<Lead>> GetLeads(Guid accountid);
        Task<Lead> GetLead(Guid leadId, Guid accountid);
        Task<Lead> UpdateLead(Lead updatedlead);
        Task<Lead> CreateLead(Lead newlead);
        Task DeleteLead(Guid leadId, Guid accountid);
    }
}
