using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sohi.Models;


namespace Sohi.Api.Models.Leads
{
    public interface ILeadsRepository
    {
        Task<IEnumerable<Lead>> GetLeads(string accountid);
        Task<Lead> GetLead(Guid id, Guid accountid);
        Task<Lead> GetLeadByEmail(string email);
        Task<Lead> DeleteLead(Guid id);

        Task<IEnumerable<Lead>> Search(string name, string email, string accountid);


        Lead Add(Lead lead);
        Lead Update(Lead lead);

        //API

        //IEnumerable<Lead> GetAllLeads(string accountid);
        Task<Lead> CreateLead(Lead lead);

    }
}
