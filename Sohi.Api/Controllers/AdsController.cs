using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sohi.Api.Models.Ads;
using Sohi.Api.Models.Social;
using Sohi.Models;

namespace Sohi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdsController : ControllerBase
    {
        private readonly IAdsRepository _adsRepository;

        public AdsController(IAdsRepository adsRepository)
        {
            _adsRepository = adsRepository;
        }

        [HttpGet("{accountid}")]
        public async Task<ActionResult<AdAccount>> GetAllAccounts(string accountid)
        {
            try
            {
                return Ok(await _adsRepository.GetAllAccounts(accountid));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }

        [HttpPost]
        public async Task<ActionResult<AdAccount>> SaveAccount(AdAccount account)
        {
            try
            {
                if (account == null)
                    return BadRequest();

                var response = await _adsRepository.SaveAccount(account);

                return response;

            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error saving the Account.");
            }
        }
    }
}
