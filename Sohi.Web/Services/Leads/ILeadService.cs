using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sohi.Models;

namespace Sohi.Web.Services.Leads
{
    public interface ILeadService
    {
        Task<IEnumerable<Lead>> GetLeads();
    }
}
