using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sohi.Api.Models.Leads;
using Sohi.Models;

namespace Sohi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeadsController : ControllerBase
    {
        private readonly ILeadsRepository _leadsRepository;

        public LeadsController(ILeadsRepository leadsRepository)
        {
            _leadsRepository = leadsRepository;
        }

        [HttpGet("{accountid:Guid}")]
        public async Task<ActionResult> GetLeads(string accountid)
        {

            try
            {
                return Ok(await _leadsRepository.GetLeads(accountid));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }

        [HttpGet("{id:Guid}/{accountid:Guid}")]
        public async Task<ActionResult<Lead>> GetLead(string id, Guid accountid)
        {
            try
            {
                Guid leadId = Guid.Parse(id);

                var result = await _leadsRepository.GetLead(leadId, accountid);

                if (result == null) return NotFound();

                return result;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Lead>> CreateLead(Lead lead)
        {
            try
            {
                if (lead == null)
                    return BadRequest();

                var existinglead = await _leadsRepository.GetLeadByEmail(lead.Email);

                if (existinglead != null)
                {
                    ModelState.AddModelError("email", "Lead email already in use");
                    return BadRequest(ModelState);
                }

                var createdLead = await _leadsRepository.CreateLead(lead);

                return CreatedAtAction(nameof(GetLead),
                    new { id = createdLead.LeadId, accountid = createdLead.AccountId }, createdLead);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error creating new lead record");
            }
        }

        [HttpPut]
        public async Task<ActionResult<Lead>> UpdateLead(Lead lead)
        {
            try
            {
                var leadToUpdate = await _leadsRepository.GetLead(lead.LeadId, lead.AccountId);

                if (leadToUpdate == null)
                    return NotFound($"Lead with Id = {lead.LeadId} not found");

                return await _leadsRepository.UpdateLead(lead);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error updating data");
            }
        }

        [HttpDelete("{id:Guid}/{accountid:Guid}")]
        public async Task<ActionResult<Lead>> DeleteLead(string id, Guid accountid)
        {
            try
            {
                Guid leadId = Guid.Parse(id);

                var leadToDelete = await _leadsRepository.GetLead(leadId, accountid);

                if (leadToDelete == null)
                {
                    return NotFound($"Lead with Id = {id} not found");
                }

                return await _leadsRepository.DeleteLead(leadId);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error deleting data");
            }
        }

        [HttpGet("{search}")]
        public async Task<ActionResult<IEnumerable<Lead>>> Search(string name, string? email, string accountid)
        {
            try
            {
                var result = await _leadsRepository.Search(name, email, accountid);

                if (result.Any())
                {
                    return Ok(result);
                }

                return NotFound();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }
    }
}
