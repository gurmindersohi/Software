using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sohi.Api.Models.Social;
using Sohi.Models;

namespace Sohi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SocialController : ControllerBase
    {
        private readonly ISocialRepository _socialRepository;

        public SocialController(ISocialRepository socialRepository)
        {
            _socialRepository = socialRepository;
        }

        [HttpPost]
        public async Task<ActionResult<SocialMedia>> SaveToken(SocialMedia account)
        {
            try
            {
                if (account == null)
                    return BadRequest();

                var response = await _socialRepository.SaveToken(account);

                return response;

            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error saving the token.");
            }
        }

        [HttpGet("{accountid}/{platform}")]
        public async Task<ActionResult<SocialMedia>> GetToken(string accountid, string platform)
        {
            try
            {

                var result = await _socialRepository.GetTokenByPlatformAsync(accountid, platform);

                if (result == null) return NotFound();

                return result;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }

        [HttpGet("{accountid}")]
        public async Task<ActionResult<SocialMedia>> GetAllTokens(string accountid)
        {
            try
            {
                return Ok(await _socialRepository.GetAllTokens(accountid));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }

        [HttpDelete("{id:Guid}")]
        public async Task<ActionResult<SocialMedia>> DeleteAccount(Guid id)
        {
            try
            {

                //var leadToDelete = await _socialRepository.GetLead(leadId, accountid);

                //if (leadToDelete == null)
                //{
                //    return NotFound($"Lead with Id = {id} not found");
                //}

                return await _socialRepository.DeleteAccount(id);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error deleting data");
            }
        }
    }
}
