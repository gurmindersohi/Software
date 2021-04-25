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

                var existinglead = _leadsRepository.GetLeadByEmail(lead.Email);

                if (existinglead != null)
                {
                    ModelState.AddModelError("email", "Employee email already in use");
                    return BadRequest(ModelState);
                }

                var createdLead = await _leadsRepository.CreateLead(lead);

                return CreatedAtAction(nameof(GetLead),
                    new { id = createdLead.LeadId, accountid = createdLead.AccountId }, createdLead);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error creating new employee record");
            }
        }

        //[HttpPut("{id}")]
        //public async Task<ActionResult<Lead>> UpdateEmployee(string id, Lead lead)
        //{
        //    try
        //    {
        //        Guid leadId = Guid.Parse(id);

        //        if (leadId != lead.LeadId)
        //            return BadRequest("Lead ID mismatch");

        //        var leadToUpdate = await _leadsRepository.GetLead(leadId);

        //        if (leadToUpdate == null)
        //            return NotFound($"Lead with Id = {id} not found");

        //        return await _leadsRepository.UpdateLead(lead);
        //    }
        //    catch (Exception)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError,
        //            "Error updating data");
        //    }
        //}

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
