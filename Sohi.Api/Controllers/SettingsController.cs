using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sohi.Api.Models.Settings;
using Sohi.Models;

namespace Sohi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsRepository _settingRepository;

        public SettingsController(ISettingsRepository settingRepository)
        {
            _settingRepository = settingRepository;
        }

        [HttpGet("{accountid:Guid}")]
        public async Task<ActionResult<Account>> GetAccount(Guid accountid)
        {
            try
            {
                var result = await _settingRepository.GetAccount(accountid);

                if (result == null) return NotFound();

                return result;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }

        [HttpPut]
        public async Task<ActionResult<Account>> UpdateAccount(Account account)
        {
            try
            {
                var accountToUpdate = await _settingRepository.GetAccount(account.AccountId);

                if (accountToUpdate == null)
                    return NotFound($"Account with Id = {account.AccountId} not found");

                return await _settingRepository.UpdateAccount(account);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error updating data");
            }
        }
    }
}
