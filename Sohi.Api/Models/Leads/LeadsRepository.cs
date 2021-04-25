using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Sohi.Models;

namespace Sohi.Api.Models.Leads
{
    public class LeadsRepository : ILeadsRepository
    {
        private readonly AppDbContext context;

        public LeadsRepository(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<Lead> CreateLead(Lead lead)
        {
            var result = await context.Leads.AddAsync(lead);

            await context.SaveChangesAsync();

            return result.Entity;
        }

        public async Task<Lead> DeleteLead(Guid id)
        {
            var result = await context.Leads.FirstOrDefaultAsync(l => l.LeadId == id);

            if (result != null) {
                context.Leads.Remove(result);
                await context.SaveChangesAsync();
                return result;
            }

            return null;

        }

        public async Task<Lead> GetLeadById(Guid id)
        {
            return await context.Leads.FirstOrDefaultAsync(l => l.LeadId == id);
        }

        public Lead Update(Lead user)
        {
            throw new NotImplementedException();
        }


        public async Task<IEnumerable<Lead>> GetLeads(string accountid)
        {
            Guid id = new Guid(accountid);

            List<Lead> leads = new List<Lead>();

            leads = await context.Leads.Where(a => a.AccountId == id).ToListAsync();
            return leads;
        }

        public Lead Add(Lead lead)
        {
            throw new NotImplementedException();
        }

        public async Task<Lead> GetLead(Guid id, Guid accountid)
        {
            return await context.Leads.FirstOrDefaultAsync(l => l.LeadId == id && l.AccountId == accountid);
        }

        public async Task<Lead> GetLeadByEmail(string email)
        {
            return await context.Leads.FirstOrDefaultAsync(l => l.Email == email);
        }

        public async Task<IEnumerable<Lead>> Search(string name, string email, string accountid)
        {
            Guid id = new Guid(accountid);

            IQueryable<Lead> query = context.Leads;

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(l => l.FirstName.Contains(name)
                            || l.LastName.Contains(name) && l.AccountId == id);
            }

            if (email != null)
            {
                query = query.Where(l => l.Email == email && l.AccountId == id);
            }

            return await query.ToListAsync();
        }
    }
}
